using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PriceWatch.Modules.Prices.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tooling (dotnet ef migrations add).
/// </summary>
internal sealed class PricesDbContextFactory : IDesignTimeDbContextFactory<PricesDbContext>
{
    public PricesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PricesDbContext>()
            .UseMySql(
                "Server=localhost;Port=3306;Database=pricewatch;Uid=root;Pwd=root;",
                ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=pricewatch;Uid=root;Pwd=root;"),
                mySql => mySql.MigrationsHistoryTable("__ef_migrations_prices"))
            .Options;

        return new PricesDbContext(options);
    }
}
