using FluentAssertions;
using NSubstitute;
using PriceWatch.Modules.Prices.Application.Queries.GetStationPrices;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Repositories;

namespace PriceWatch.UnitTests.Application.QueryHandlers;

public class GetStationPricesQueryHandlerTests
{
    private readonly IStationPriceRepository _repository = Substitute.For<IStationPriceRepository>();
    private readonly GetStationPricesQueryHandler _handler;

    public GetStationPricesQueryHandlerTests()
    {
        _handler = new GetStationPricesQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_StationFound_ShouldReturnMappedDto()
    {
        var station = StationPrice.Create("EXT-001", "Station A", "10 rue de Paris", "Paris", "75001", 48.856, 2.352);
        station.UpsertFuelPrice("Gazole", 1.85m, DateTime.UtcNow);

        _repository.GetByExternalIdAsync("EXT-001", Arg.Any<CancellationToken>())
            .Returns(station);

        var result = await _handler.Handle(
            new GetStationPricesQuery("EXT-001"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExternalStationId.Should().Be("EXT-001");
        result.Value.StationName.Should().Be("Station A");
        result.Value.FuelPrices.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_StationNotFound_ShouldReturnNotFoundError()
    {
        _repository.GetByExternalIdAsync("EXT-UNKNOWN", Arg.Any<CancellationToken>())
            .Returns((StationPrice?)null);

        var result = await _handler.Handle(
            new GetStationPricesQuery("EXT-UNKNOWN"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
        result.Error.Description.Should().Contain("EXT-UNKNOWN");
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithExternalId()
    {
        _repository.GetByExternalIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((StationPrice?)null);

        await _handler.Handle(
            new GetStationPricesQuery("EXT-XYZ"),
            CancellationToken.None);

        await _repository.Received(1).GetByExternalIdAsync(
            "EXT-XYZ",
            Arg.Any<CancellationToken>());
    }
}
