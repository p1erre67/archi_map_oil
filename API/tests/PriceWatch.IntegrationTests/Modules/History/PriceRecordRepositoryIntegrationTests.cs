using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PriceWatch.IntegrationTests.Infrastructure;
using PriceWatch.Modules.History.Application.Interfaces;
using PriceWatch.Modules.History.Domain.Entities;
using PriceWatch.Modules.History.Domain.Repositories;

namespace PriceWatch.IntegrationTests.Modules.History;

/// <summary>
/// Tests d'integration du PriceRecordRepository sur un vrai Postgres (Testcontainers).
/// Utilise un externalStationId unique par test pour eviter les interferences.
/// </summary>
[Collection("Integration")]
public class PriceRecordRepositoryIntegrationTests : IntegrationTestBase
{
    private readonly PriceWatchWebApplicationFactory _factory;

    public PriceRecordRepositoryIntegrationTests(PriceWatchWebApplicationFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetByStationAsync_WithFuelTypeSP95_ShouldIncludeSP95E10Aliases()
    {
        const string stationId = "REPO-SP95-ALIAS";
        var recordedAt = DateTime.UtcNow;

        await SeedRecords(
            PriceRecord.Create(stationId, "A", "Paris", "SP95",     1.80m, recordedAt),
            PriceRecord.Create(stationId, "A", "Paris", "SP95-E10", 1.85m, recordedAt),
            PriceRecord.Create(stationId, "A", "Paris", "Gazole",   1.70m, recordedAt));

        await using var scope = _factory.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPriceRecordRepository>();

        var result = await repo.GetByStationAsync(stationId, fuelType: "SP95");

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.FuelType == "SP95" || r.FuelType == "SP95-E10");
    }

    [Fact]
    public async Task GetGlobalAveragesAsync_ShouldFilterByDateRange()
    {
        const string stationId = "REPO-DATE-RANGE";
        var now = DateTime.UtcNow;

        await SeedRecords(
            PriceRecord.Create(stationId, "A", "Paris", "Gazole", 1.70m, now.AddDays(-10)),
            PriceRecord.Create(stationId, "A", "Paris", "Gazole", 1.75m, now.AddDays(-5)),
            PriceRecord.Create(stationId, "A", "Paris", "Gazole", 1.80m, now.AddDays(-1)));

        await using var scope = _factory.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPriceRecordRepository>();

        var result = await repo.GetGlobalAveragesAsync(
            "Gazole",
            now.AddDays(-6),
            now);

        // Doit contenir les deux derniers records (5 jours et 1 jour)
        result.Where(r => r.ExternalStationId == stationId).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetGlobalAveragesAsync_WithStationIds_ShouldFilterByIds()
    {
        var now = DateTime.UtcNow;

        await SeedRecords(
            PriceRecord.Create("REPO-FILTER-IDS-A", "A", "Paris", "Gazole", 1.70m, now),
            PriceRecord.Create("REPO-FILTER-IDS-B", "B", "Lyon",  "Gazole", 1.75m, now),
            PriceRecord.Create("REPO-FILTER-IDS-C", "C", "Nice",  "Gazole", 1.80m, now));

        await using var scope = _factory.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPriceRecordRepository>();

        var result = await repo.GetGlobalAveragesAsync(
            "Gazole",
            now.AddDays(-1),
            now.AddDays(1),
            stationIds: new List<string> { "REPO-FILTER-IDS-A", "REPO-FILTER-IDS-C" });

        var matchedIds = result
            .Where(r => r.ExternalStationId.StartsWith("REPO-FILTER-IDS-"))
            .Select(r => r.ExternalStationId)
            .ToList();

        matchedIds.Should().Contain("REPO-FILTER-IDS-A");
        matchedIds.Should().Contain("REPO-FILTER-IDS-C");
        matchedIds.Should().NotContain("REPO-FILTER-IDS-B");
    }

    private async Task SeedRecords(params PriceRecord[] records)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPriceRecordRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IHistoryUnitOfWork>();

        repo.AddRange(records);
        await uow.SaveChangesAsync();
    }
}
