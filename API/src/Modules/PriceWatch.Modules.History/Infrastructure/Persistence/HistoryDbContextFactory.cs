using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PriceWatch.Modules.History.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Usage: dotnet ef migrations add Init --project src/Modules/PriceWatch.Modules.History --context HistoryDbContext
/// </summary>
internal sealed class HistoryDbContextFactory : IDesignTimeDbContextFactory<HistoryDbContext>
{
    public HistoryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HistoryDbContext>()
            .UseMySql(
                "Server=localhost;Database=pricewatch;User=pricewatch;Password=pricewatch;",
                ServerVersion.AutoDetect("Server=localhost;Database=pricewatch;User=pricewatch;Password=pricewatch;"))
            .Options;

        return new HistoryDbContext(options);
    }
}
