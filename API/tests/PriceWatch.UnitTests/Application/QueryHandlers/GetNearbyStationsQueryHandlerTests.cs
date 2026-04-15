using FluentAssertions;
using NSubstitute;
using PriceWatch.Modules.Prices.Application.Queries.GetNearbyStations;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Repositories;

namespace PriceWatch.UnitTests.Application.QueryHandlers;

public class GetNearbyStationsQueryHandlerTests
{
    private readonly IStationPriceRepository _repository = Substitute.For<IStationPriceRepository>();
    private readonly GetNearbyStationsQueryHandler _handler;

    public GetNearbyStationsQueryHandlerTests()
    {
        _handler = new GetNearbyStationsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithNearbyStations_ShouldReturnMappedDtos()
    {
        var station = StationPrice.Create("EXT-001", "Station A", "10 rue de Paris", "Paris", "75001", 48.856, 2.352);
        station.UpsertFuelPrice("Gazole", 1.85m, DateTime.UtcNow);
        station.UpsertFuelPrice("SP95", 1.92m, DateTime.UtcNow);

        //fake total du repo, on ne test que la logique du handler
        //parametrage du repo : méthode GetNearbyAsync avec n'importe quelle latitude, longitude, rayon à 10km et token, retourne une liste avec notre station 
        _repository.GetNearbyAsync(Arg.Any<double>(), Arg.Any<double>(), 10, Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice> { station });

        var result = await _handler.Handle(
            new GetNearbyStationsQuery(48.856, 2.352, 10),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var dto = result.Value[0];
        dto.ExternalStationId.Should().Be("EXT-001");
        dto.StationName.Should().Be("Station A");
        dto.FuelPrices.Should().HaveCount(2);
        dto.FuelPrices.Should().Contain(fp => fp.FuelType == "Gazole" && fp.PricePerLiter == 1.85m);
    }

    [Fact]
    public async Task Handle_NoStations_ShouldReturnEmptyList()
    {
        _repository.GetNearbyAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice>());

        var result = await _handler.Handle(
            new GetNearbyStationsQuery(48.856, 2.352, 10),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectParams()
    {
        _repository.GetNearbyAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice>());

        await _handler.Handle(
            new GetNearbyStationsQuery(48.856, 2.352, 15),
            CancellationToken.None);

        await _repository.Received(1).GetNearbyAsync(
            48.856,
            2.352,
            15,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPropagateCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        _repository.GetNearbyAsync(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice>());

        await _handler.Handle(
            new GetNearbyStationsQuery(48.856, 2.352, 10),
            cts.Token);

        await _repository.Received(1).GetNearbyAsync(
            Arg.Any<double>(),
            Arg.Any<double>(),
            Arg.Any<double>(),
            cts.Token);
    }
}
