using Microsoft.EntityFrameworkCore;
using PriceWatch.Modules.Cards.Application.Interfaces;
using PriceWatch.Modules.Cards.Domain.Entities;
using PriceWatch.Modules.Cards.Infrastructure.Persistence.Configurations;

namespace PriceWatch.Modules.Cards.Infrastructure.Persistence;

internal sealed class CardsDbContext : DbContext, ICardsUnitOfWork
{
    public CardsDbContext(DbContextOptions<CardsDbContext> options) : base(options) { }

    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CardConfiguration());
    }
}
