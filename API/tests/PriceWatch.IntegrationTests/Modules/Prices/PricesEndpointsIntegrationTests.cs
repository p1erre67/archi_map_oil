using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PriceWatch.IntegrationTests.Infrastructure;
using PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;
using PriceWatch.Modules.Prices.Application.DTOs;

namespace PriceWatch.IntegrationTests.Modules.Prices;

[Collection("Integration")]
public class PricesEndpointsIntegrationTests : IntegrationTestBase
{
    public PricesEndpointsIntegrationTests(PriceWatchWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCheapest_ShouldReturn200AndJson()
    {
        // Seed : un minimum de donnees via une commande
        await Sender.Send(new SyncStationPricesCommand(new List<ExternalStationDto>
        {
            new(
                Id: "ENDPOINT-TEST-001",
                Name: "Endpoint Test",
                Address: "addr",
                City: "Paris",
                PostalCode: "75001",
                Lat: 48.8,
                Lon: 2.3,
                FuelPrices: new List<ExternalFuelPriceDto>
                {
                    new("SP98", 1.95m, DateTime.UtcNow),
                })
        }));

        var client = CreateHttpClient();
        var response = await client.GetAsync("/api/prices/cheapest?fuelType=SP98&limit=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stations = await response.Content.ReadFromJsonAsync<List<StationPriceDto>>();
        stations.Should().NotBeNull();
        stations!.Should().Contain(s => s.ExternalStationId == "ENDPOINT-TEST-001");
    }

    [Fact]
    public async Task GetStationPrices_UnknownStation_ShouldReturn404()
    {
        var client = CreateHttpClient();
        var response = await client.GetAsync("/api/prices/stations/DOES-NOT-EXIST-XYZ");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
