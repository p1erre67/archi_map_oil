using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PriceWatch.Modules.Cards.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tooling (dotnet ef migrations add).
/// </summary>
internal sealed class CardsDbContextFactory : IDesignTimeDbContextFactory<CardsDbContext>
{
    public CardsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CardsDbContext>()
            .UseMySql(
                "Server=localhost;Port=3306;Database=pricewatch;Uid=root;Pwd=root;",
                ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=pricewatch;Uid=root;Pwd=root;"),
                mySql => mySql.MigrationsHistoryTable("__ef_migrations_cards"))
            .Options;

        return new CardsDbContext(options);
    }
}
