using MediatR;
using Microsoft.Extensions.Logging;
using PriceWatch.Modules.Prices.Domain.Events;
using PriceWatch.Modules.Prices.Domain.Repositories;

namespace PriceWatch.Modules.Prices.Application.EventHandlers;

internal sealed class DetectPriceAnomalyHandler : INotificationHandler<StationPricesSyncedEvent>
{
    private const decimal MinExpectedPrice = 0.50m;
    private const decimal MaxExpectedPrice = 3.00m;

    private readonly IStationPriceRepository _repository;
    private readonly ILogger<DetectPriceAnomalyHandler> _logger;

    public DetectPriceAnomalyHandler(
        IStationPriceRepository repository,
        ILogger<DetectPriceAnomalyHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(StationPricesSyncedEvent notification, CancellationToken cancellationToken)
    {
        var stations = await _repository.GetAllAsync(cancellationToken);

        foreach (var station in stations)
        {
            foreach (var fuel in station.FuelPrices)
            {
                if (fuel.PricePerLiter < MinExpectedPrice)
                {
                    _logger.LogWarning(
                        "[Anomaly] {StationName} — {FuelType} at {Price}€/L is abnormally LOW.",
                        station.StationName, fuel.FuelType, fuel.PricePerLiter);
                }
                else if (fuel.PricePerLiter > MaxExpectedPrice)
                {
                    _logger.LogWarning(
                        "[Anomaly] {StationName} — {FuelType} at {Price}€/L is abnormally HIGH.",
                        station.StationName, fuel.FuelType, fuel.PricePerLiter);
                }
            }
        }
    }
}
