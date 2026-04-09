using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PriceWatch.Modules.Prices.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tooling (dotnet ef migrations add).
/// Reads the connection string from the API project's appsettings.json and user secrets.
/// </summary>
internal sealed class PricesDbContextFactory : IDesignTimeDbContextFactory<PricesDbContext>
{
    public PricesDbContext CreateDbContext(string[] args)
    {
        // Cherche le projet API en remontant depuis le cwd
        var apiProjectPath = FindApiProjectPath();

        var config = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets("05fffc32-cee8-415d-bea7-90223e297caa")
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("PriceWatch")
            ?? throw new InvalidOperationException("Connection string 'PriceWatch' not found.");

        var options = new DbContextOptionsBuilder<PricesDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_prices"))
            .Options;

        return new PricesDbContext(options, new NullPublisher());
    }

    private static string FindApiProjectPath()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "PriceWatch.Api");
            if (Directory.Exists(candidate))
                return candidate;

            // cas où on est déjà à l'intérieur de src/
            candidate = Path.Combine(dir.FullName, "PriceWatch.Api");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "appsettings.json")))
                return candidate;

            dir = dir.Parent;
        }
        throw new InvalidOperationException("PriceWatch.Api project not found.");
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
