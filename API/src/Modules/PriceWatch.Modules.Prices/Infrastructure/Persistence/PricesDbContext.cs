using MediatR;
using Microsoft.EntityFrameworkCore;
using PriceWatch.Modules.Prices.Application.Interfaces;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Infrastructure.Persistence.Configurations;
using PriceWatch.SharedKernel.Domain.Primitives;

namespace PriceWatch.Modules.Prices.Infrastructure.Persistence;

internal sealed class PricesDbContext : DbContext, IPricesUnitOfWork
{
    private readonly IPublisher _publisher;

    public PricesDbContext(DbContextOptions<PricesDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    public DbSet<StationPrice> StationPrices => Set<StationPrice>();
    public DbSet<Brand> Brands => Set<Brand>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StationPriceConfiguration());
        modelBuilder.ApplyConfiguration(new BrandConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Collecter les domain events avant le save
        var aggregates = ChangeTracker.Entries()
            .Where(e => e.Entity is IHasDomainEvents)
            .Select(e => (IHasDomainEvents)e.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // 2. Clear les events pour eviter un double dispatch
        aggregates.ForEach(a => a.ClearDomainEvents());

        // 3. Persister les changements
        var result = await base.SaveChangesAsync(cancellationToken);

        // 4. Publier les events apres le save reussi
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
