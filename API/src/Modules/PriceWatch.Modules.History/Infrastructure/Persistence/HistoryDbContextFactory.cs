using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PriceWatch.Modules.History.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Reads the connection string from the API project's appsettings.json and user secrets.
/// </summary>
internal sealed class HistoryDbContextFactory : IDesignTimeDbContextFactory<HistoryDbContext>
{
    public HistoryDbContext CreateDbContext(string[] args)
    {
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

        var options = new DbContextOptionsBuilder<HistoryDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history"))
            .Options;

        return new HistoryDbContext(options);
    }

    private static string FindApiProjectPath()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "PriceWatch.Api");
            if (Directory.Exists(candidate))
                return candidate;

            candidate = Path.Combine(dir.FullName, "PriceWatch.Api");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "appsettings.json")))
                return candidate;

            dir = dir.Parent;
        }
        throw new InvalidOperationException("PriceWatch.Api project not found.");
    }
}
