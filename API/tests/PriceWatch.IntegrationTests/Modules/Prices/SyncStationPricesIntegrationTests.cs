using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PriceWatch.IntegrationTests.Infrastructure;
using PriceWatch.Modules.History.Infrastructure.Persistence;
using PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.Modules.Prices.Infrastructure.Persistence;

namespace PriceWatch.IntegrationTests.Modules.Prices;

[Collection("Integration")]
public class SyncStationPricesIntegrationTests : IntegrationTestBase
{
    private readonly PriceWatchWebApplicationFactory _factory;

    public SyncStationPricesIntegrationTests(PriceWatchWebApplicationFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Sync_ShouldInsertStationsInPricesDatabase()
    {
        var command = new SyncStationPricesCommand(new List<ExternalStationDto>
        {
            new(
                Id: "TEST-SYNC-001",
                Name: "Sync Test Station",
                Address: "1 Test Street",
                City: "Paris",
                PostalCode: "75001",
                Lat: 48.8566,
                Lon: 2.3522,
                FuelPrices: new List<ExternalFuelPriceDto>
                {
                    new("Gazole", 1.85m, DateTime.UtcNow),
                    new("SP95", 1.92m, DateTime.UtcNow),
                })
        });

        var result = await Sender.Send(command);

        result.IsSuccess.Should().BeTrue();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PricesDbContext>();
        var station = await db.StationPrices
            .Include(s => s.FuelPrices)
            .FirstOrDefaultAsync(s => s.ExternalStationId == "TEST-SYNC-001");

        station.Should().NotBeNull();
        station!.FuelPrices.Should().HaveCount(2);
    }

    [Fact]
    public async Task Sync_ShouldPublishIntegrationEvent_AndRecordInHistory()
    {
        var command = new SyncStationPricesCommand(new List<ExternalStationDto>
        {
            new(
                Id: "TEST-HIST-001",
                Name: "History Flow Station",
                Address: "2 Test Street",
                City: "Lyon",
                PostalCode: "69001",
                Lat: 45.764,
                Lon: 4.835,
                FuelPrices: new List<ExternalFuelPriceDto>
                {
                    new("Gazole", 1.82m, DateTime.UtcNow),
                })
        });

        await Sender.Send(command);

        // Verifie que le module History a recu l'integration event et stocke les records
        await using var scope = _factory.Services.CreateAsyncScope();
        var historyDb = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        var records = await historyDb.PriceRecords
            .Where(r => r.ExternalStationId == "TEST-HIST-001")
            .ToListAsync();

        records.Should().NotBeEmpty();
        records.Should().Contain(r => r.FuelType == "Gazole" && r.PricePerLiter == 1.82m);
    }

    [Fact]
    public async Task Sync_ExistingStation_ShouldUpdateInsteadOfDuplicate()
    {
        var externalId = "TEST-UPDATE-001";

        var first = new SyncStationPricesCommand(new List<ExternalStationDto>
        {
            new(
                Id: externalId,
                Name: "Old Name",
                Address: "addr",
                City: "Paris",
                PostalCode: "75001",
                Lat: 48.8,
                Lon: 2.3,
                FuelPrices: new List<ExternalFuelPriceDto>
                {
                    new("Gazole", 1.80m, DateTime.UtcNow),
                })
        });
        await Sender.Send(first);

        var second = new SyncStationPricesCommand(new List<ExternalStationDto>
        {
            new(
                Id: externalId,
                Name: "New Name",
                Address: "addr",
                City: "Paris",
                PostalCode: "75001",
                Lat: 48.8,
                Lon: 2.3,
                FuelPrices: new List<ExternalFuelPriceDto>
                {
                    new("Gazole", 1.90m, DateTime.UtcNow),
                })
        });
        await Sender.Send(second);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PricesDbContext>();
        var stations = await db.StationPrices
            .Where(s => s.ExternalStationId == externalId)
            .ToListAsync();

        stations.Should().HaveCount(1);
        stations[0].StationName.Should().Be("New Name");
    }
}
