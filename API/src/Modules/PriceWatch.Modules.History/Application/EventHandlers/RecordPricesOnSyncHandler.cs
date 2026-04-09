using MediatR;
using Microsoft.Extensions.Logging;
using PriceWatch.Modules.History.Application.Interfaces;
using PriceWatch.Modules.History.Domain.Entities;
using PriceWatch.Modules.History.Domain.Repositories;
using PriceWatch.SharedKernel.Application.Events;

namespace PriceWatch.Modules.History.Application.EventHandlers;

internal sealed class RecordPricesOnSyncHandler : INotificationHandler<StationPricesSyncedIntegrationEvent>
{
    private readonly IPriceRecordRepository _repository;
    private readonly IHistoryUnitOfWork _unitOfWork;
    private readonly ILogger<RecordPricesOnSyncHandler> _logger;

    public RecordPricesOnSyncHandler(
        IPriceRecordRepository repository,
        IHistoryUnitOfWork unitOfWork,
        ILogger<RecordPricesOnSyncHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        StationPricesSyncedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var records = notification.Stations
            .SelectMany(station => station.FuelPrices.Select(fuel =>
                PriceRecord.Create(
                    station.ExternalStationId,
                    station.StationName,
                    station.City,
                    fuel.FuelType,
                    fuel.PricePerLiter,
                    EnsureUtc(fuel.UpdatedAt))))
            .ToList();

        _repository.AddRange(records);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[IntegrationEvent] StationPricesSynced — recorded {RecordCount} price history entries (EventId: {EventId}).",
            records.Count,
            notification.EventId);
    }

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
