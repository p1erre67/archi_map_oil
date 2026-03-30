using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PriceWatch.Modules.Cards.Infrastructure.Persistence;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.Modules.Prices.Application.Interfaces;
using PriceWatch.Modules.Prices.Infrastructure.Persistence;
using Testcontainers.MySql;

namespace PriceWatch.IntegrationTests.Infrastructure;

/// <summary>
/// Shared fixture for the "Integration" xUnit collection.
/// Starts a MariaDB 11.4 Testcontainer once for the entire test run,
/// overrides the connection string, and runs EF migrations on first use.
/// </summary>
[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<PriceWatchWebApplicationFactory> { }

public sealed class PriceWatchWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MySqlContainer _db = new MySqlBuilder()
        .WithImage("mariadb:11.4")
        .WithDatabase("pricewatch")
        .WithUsername("root")
        .WithPassword("root")
        .Build();

    // ── IAsyncLifetime ────────────────────────────────────────────────────────

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _db.StartAsync();
        await MigrateAllAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _db.DisposeAsync();
        await base.DisposeAsync();
    }

    // ── WebApplicationFactory ─────────────────────────────────────────────────

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override the connection string so every module's DbContext
        // points to the Testcontainers MariaDB instance.
        builder.UseSetting("ConnectionStrings:PriceWatch", _db.GetConnectionString());

        builder.ConfigureServices(services =>
        {
            // Replace the real IFuelPriceApiClient with a test double
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IFuelPriceApiClient));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton<IFuelPriceApiClient, FakeFuelPriceApiClient>();
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task MigrateAllAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var sp = scope.ServiceProvider;

        await sp.GetRequiredService<PricesDbContext>().Database.MigrateAsync();
        await sp.GetRequiredService<CardsDbContext>().Database.MigrateAsync();
    }
}

/// <summary>
/// Test double for IFuelPriceApiClient returning deterministic fake data.
/// </summary>
internal sealed class FakeFuelPriceApiClient : IFuelPriceApiClient
{
    public Task<IReadOnlyList<ExternalStationDto>> GetStationsAsync(
        double lat, double lon, int radiusKm, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ExternalStationDto> stations = new List<ExternalStationDto>
        {
            new ExternalStationDto(
                "FAKE-001",
                "Station Test Paris",
                "1 Rue de la Paix",
                "Paris",
                "75001",
                48.8566,
                2.3522,
                new List<ExternalFuelPriceDto>
                {
                    new ExternalFuelPriceDto("SP95", 1.85m, DateTime.UtcNow),
                    new ExternalFuelPriceDto("Gazole", 1.72m, DateTime.UtcNow)
                })
        };

        return Task.FromResult(stations);
    }
}
