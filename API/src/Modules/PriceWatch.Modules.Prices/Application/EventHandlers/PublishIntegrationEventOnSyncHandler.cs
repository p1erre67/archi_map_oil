using MediatR;
using Microsoft.Extensions.Logging;
using PriceWatch.Modules.Prices.Domain.Events;
using PriceWatch.Modules.Prices.Domain.Repositories;
using PriceWatch.SharedKernel.Application.Events;

namespace PriceWatch.Modules.Prices.Application.EventHandlers;

/// <summary>
/// Listens to the internal DomainEvent and publishes an IntegrationEvent
/// so other modules (e.g. History) can react without coupling to the Prices domain.
/// </summary>
internal sealed class PublishIntegrationEventOnSyncHandler : INotificationHandler<StationPricesSyncedEvent>
{
    private readonly IStationPriceRepository _repository;
    private readonly IPublisher _publisher;
    private readonly ILogger<PublishIntegrationEventOnSyncHandler> _logger;

    public PublishIntegrationEventOnSyncHandler(
        IStationPriceRepository repository,
        IPublisher publisher,
        ILogger<PublishIntegrationEventOnSyncHandler> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(StationPricesSyncedEvent notification, CancellationToken cancellationToken)
    {
        var stations = await _repository.GetAllAsync(cancellationToken);

        var snapshots = stations.Select(s => new SyncedStationSnapshot(
            s.ExternalStationId,
            s.StationName,
            s.City,
            s.FuelPrices.Select(fp => new SyncedFuelPriceSnapshot(
                fp.FuelType,
                fp.PricePerLiter,
                fp.UpdatedAt)).ToList())).ToList();

        var integrationEvent = new StationPricesSyncedIntegrationEvent
        {
            Stations = snapshots
        };

        _logger.LogInformation(
            "[IntegrationEvent] Publishing StationPricesSyncedIntegrationEvent with {StationCount} station(s).",
            snapshots.Count);

        await _publisher.Publish(integrationEvent, cancellationToken);
    }
}
