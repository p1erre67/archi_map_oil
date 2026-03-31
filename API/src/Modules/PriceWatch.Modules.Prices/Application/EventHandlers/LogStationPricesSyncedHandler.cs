using MediatR;
using Microsoft.Extensions.Logging;
using PriceWatch.Modules.Prices.Domain.Events;

namespace PriceWatch.Modules.Prices.Application.EventHandlers;

internal sealed class LogStationPricesSyncedHandler : INotificationHandler<StationPricesSyncedEvent>
{
    private readonly ILogger<LogStationPricesSyncedHandler> _logger;

    public LogStationPricesSyncedHandler(ILogger<LogStationPricesSyncedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(StationPricesSyncedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DomainEvent] StationPricesSynced — {StationCount} station(s) synchronized (EventId: {EventId}).",
            notification.StationCount,
            notification.EventId);

        return Task.CompletedTask;
    }
}
