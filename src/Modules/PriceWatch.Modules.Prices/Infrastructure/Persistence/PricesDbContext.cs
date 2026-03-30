using Microsoft.EntityFrameworkCore;
using PriceWatch.Modules.Prices.Application.Interfaces;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Infrastructure.Persistence.Configurations;

namespace PriceWatch.Modules.Prices.Infrastructure.Persistence;

internal sealed class PricesDbContext : DbContext, IPricesUnitOfWork
{
    public PricesDbContext(DbContextOptions<PricesDbContext> options) : base(options) { }

    public DbSet<StationPrice> StationPrices => Set<StationPrice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StationPriceConfiguration());
    }
}
