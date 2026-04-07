using PriceWatch.Modules.History.Domain.ValueObjects;
using PriceWatch.SharedKernel.Domain.Primitives;

namespace PriceWatch.Modules.History.Domain.Entities;

/// <summary>
/// Represents a single fuel price observation at a given point in time.
/// One record per station + fuel type + sync instant.
/// Immutable after creation — history is append-only.
/// </summary>
public sealed class PriceRecord : Entity<PriceRecordId>
{
    public string ExternalStationId { get; private set; }
    public string StationName { get; private set; }
    public string City { get; private set; }
    public string FuelType { get; private set; }
    public decimal PricePerLiter { get; private set; }
    public DateTime RecordedAt { get; private set; }

    private PriceRecord(
        PriceRecordId id,
        string externalStationId,
        string stationName,
        string city,
        string fuelType,
        decimal pricePerLiter,
        DateTime recordedAt) : base(id)
    {
        ExternalStationId = externalStationId;
        StationName = stationName;
        City = city;
        FuelType = fuelType;
        PricePerLiter = pricePerLiter;
        RecordedAt = recordedAt;
    }

    public static PriceRecord Create(
        string externalStationId,
        string stationName,
        string city,
        string fuelType,
        decimal pricePerLiter,
        DateTime recordedAt)
    {
        return new PriceRecord(
            PriceRecordId.New(),
            externalStationId,
            stationName,
            city,
            fuelType,
            pricePerLiter,
            recordedAt);
    }

#pragma warning disable CS8618
    private PriceRecord() : base(default!) { }
#pragma warning restore CS8618
}
