using MediatR;
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
                "Server=localhost;Port=3306;Database=pricewatch;Uid=root;Pwd=gescar;",
                ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=pricewatch;Uid=root;Pwd=gescar;"),
                mySql => mySql.MigrationsHistoryTable("__ef_migrations_prices"))
            .Options;

        return new PricesDbContext(options, new NullPublisher());
    }

    /// <summary>
    /// No-op publisher — EF tooling never triggers domain events.
    /// </summary>
    private sealed class NullPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
