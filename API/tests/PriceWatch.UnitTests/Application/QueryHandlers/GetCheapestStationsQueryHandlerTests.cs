using FluentAssertions;
using NSubstitute;
using PriceWatch.Modules.Prices.Application.Queries.GetCheapestStations;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Repositories;

namespace PriceWatch.UnitTests.Application.QueryHandlers;

public class GetCheapestStationsQueryHandlerTests
{
    private readonly IStationPriceRepository _repository = Substitute.For<IStationPriceRepository>();
    private readonly GetCheapestStationsQueryHandler _handler;

    public GetCheapestStationsQueryHandlerTests()
    {
        _handler = new GetCheapestStationsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithStations_ShouldReturnMappedDtos()
    {
        var s1 = StationPrice.Create("EXT-001", "Station A", "addr", "Paris", "75001", 48.856, 2.352);
        s1.UpsertFuelPrice("SP95", 1.75m, DateTime.UtcNow);
        var s2 = StationPrice.Create("EXT-002", "Station B", "addr", "Lyon", "69001", 45.75, 4.85);
        s2.UpsertFuelPrice("SP95", 1.80m, DateTime.UtcNow);

        _repository.GetCheapestByFuelTypeAsync("SP95", 10, Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice> { s1, s2 });

        var result = await _handler.Handle(
            new GetCheapestStationsQuery("SP95", 10),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(dto => dto.ExternalStationId).Should().ContainInOrder("EXT-001", "EXT-002");
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithFuelTypeAndLimit()
    {
        _repository.GetCheapestByFuelTypeAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice>());

        await _handler.Handle(
            new GetCheapestStationsQuery("Gazole", 5),
            CancellationToken.None);

        await _repository.Received(1).GetCheapestByFuelTypeAsync(
            "Gazole",
            5,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturnEmptyList()
    {
        _repository.GetCheapestByFuelTypeAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice>());

        var result = await _handler.Handle(
            new GetCheapestStationsQuery("SP98"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
