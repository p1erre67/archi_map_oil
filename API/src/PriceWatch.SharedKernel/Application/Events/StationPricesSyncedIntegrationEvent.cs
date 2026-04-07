namespace PriceWatch.SharedKernel.Application.Events;

/// <summary>
/// Integration event published after a successful price sync.
/// Carries the price snapshots so consuming modules (e.g. History) can store them
/// without querying back into the Prices module.
/// </summary>
public sealed record StationPricesSyncedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;

    public required IReadOnlyList<SyncedStationSnapshot> Stations { get; init; }
}

public sealed record SyncedStationSnapshot(
    string ExternalStationId,
    string StationName,
    string City,
    IReadOnlyList<SyncedFuelPriceSnapshot> FuelPrices);

public sealed record SyncedFuelPriceSnapshot(
    string FuelType,
    decimal PricePerLiter,
    DateTime UpdatedAt);
