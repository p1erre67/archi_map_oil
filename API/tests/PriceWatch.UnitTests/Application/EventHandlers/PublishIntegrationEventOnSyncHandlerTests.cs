using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PriceWatch.Modules.Prices.Application.EventHandlers;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Events;
using PriceWatch.Modules.Prices.Domain.Repositories;
using PriceWatch.SharedKernel.Application.Events;

namespace PriceWatch.UnitTests.Application.EventHandlers;

public class PublishIntegrationEventOnSyncHandlerTests
{
    private readonly IStationPriceRepository _repository = Substitute.For<IStationPriceRepository>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly PublishIntegrationEventOnSyncHandler _handler;

    public PublishIntegrationEventOnSyncHandlerTests()
    {
        _handler = new PublishIntegrationEventOnSyncHandler(
            _repository, _publisher, NullLoggerFactory.Instance.CreateLogger<PublishIntegrationEventOnSyncHandler>());
    }

    [Fact]
    public async Task Handle_ShouldPublishIntegrationEvent()
    {
        var station = StationPrice.Create("EXT-001", "Station A", "addr", "Paris", "75001", 48.856, 2.352);
        station.UpsertFuelPrice("Gazole", 1.85m, DateTime.UtcNow);

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice> { station });

        await _handler.Handle(new StationPricesSyncedEvent(1), CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<StationPricesSyncedIntegrationEvent>(e => e.Stations.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapStationDataCorrectly()
    {
        var station = StationPrice.Create("EXT-001", "Station A", "addr", "Paris", "75001", 48.856, 2.352);
        station.UpsertFuelPrice("Gazole", 1.85m, DateTime.UtcNow);
        station.UpsertFuelPrice("SP95", 1.92m, DateTime.UtcNow);

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice> { station });

        StationPricesSyncedIntegrationEvent? captured = null;
        await _publisher.Publish(
            Arg.Do<StationPricesSyncedIntegrationEvent>(e => captured = e),
            Arg.Any<CancellationToken>());

        await _handler.Handle(new StationPricesSyncedEvent(1), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Stations[0].ExternalStationId.Should().Be("EXT-001");
        captured.Stations[0].StationName.Should().Be("Station A");
        captured.Stations[0].City.Should().Be("Paris");
        captured.Stations[0].FuelPrices.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NoStations_ShouldPublishEmptyEvent()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice>());

        await _handler.Handle(new StationPricesSyncedEvent(0), CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<StationPricesSyncedIntegrationEvent>(e => e.Stations.Count == 0),
            Arg.Any<CancellationToken>());
    }
}
