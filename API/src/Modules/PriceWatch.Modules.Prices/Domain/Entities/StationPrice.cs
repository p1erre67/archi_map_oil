using PriceWatch.Modules.Prices.Domain.Events;
using PriceWatch.Modules.Prices.Domain.ValueObjects;
using PriceWatch.SharedKernel.Domain.Primitives;

using static PriceWatch.SharedKernel.Domain.Primitives.Guard;

namespace PriceWatch.Modules.Prices.Domain.Entities;

/// <summary>
/// Aggregate root representing one station's complete price list.
/// Identified by an external API station ID.
/// </summary>
public sealed class StationPrice : AggregateRoot<StationPriceId>
{
    private readonly List<FuelPrice> _fuelPrices = [];

    public string ExternalStationId { get; private set; }
    public string StationName { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public DateTime LastUpdated { get; private set; }
    public int? BrandId { get; private set; }
    public Brand? Brand { get; private set; }

    public IReadOnlyList<FuelPrice> FuelPrices => _fuelPrices.AsReadOnly();

    private StationPrice(
        StationPriceId id,
        string externalStationId,
        string stationName,
        string address,
        string city,
        string postalCode,
        double latitude,
        double longitude) : base(id)
    {
        ExternalStationId = externalStationId;
        StationName = stationName;
        Address = address;
        City = city;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
        LastUpdated = DateTime.UtcNow;
    }

    public static StationPrice Create(
        string externalStationId,
        string stationName,
        string address,
        string city,
        string postalCode,
        double latitude,
        double longitude,
        int? brandId = null)
    {
        AgainstNullOrWhiteSpace(externalStationId, nameof(externalStationId));
        AgainstNullOrWhiteSpace(stationName, nameof(stationName));
        AgainstOutOfRange(latitude, -90, 90, nameof(latitude));
        AgainstOutOfRange(longitude, -180, 180, nameof(longitude));

        var station = new StationPrice(
            StationPriceId.New(),
            externalStationId,
            stationName,
            address,
            city,
            postalCode,
            latitude,
            longitude);
        station.BrandId = brandId;
        return station;
    }

    public void UpdateInfo(
        string stationName,
        string address,
        string city,
        string postalCode,
        double latitude,
        double longitude,
        int? brandId = null)
    {
        AgainstNullOrWhiteSpace(stationName, nameof(stationName));
        AgainstOutOfRange(latitude, -90, 90, nameof(latitude));
        AgainstOutOfRange(longitude, -180, 180, nameof(longitude));

        StationName = stationName;
        Address = address;
        City = city;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
        BrandId = brandId;
        LastUpdated = DateTime.UtcNow;
    }

    public void UpsertFuelPrice(string fuelType, decimal pricePerLiter, DateTime updatedAt)
    {
        AgainstNullOrWhiteSpace(fuelType, nameof(fuelType));
        AgainstNegativeOrZero(pricePerLiter, nameof(pricePerLiter));

        var existing = _fuelPrices.FirstOrDefault(fp => fp.FuelType == fuelType);
        if (existing is not null)
        {
            existing.UpdatePrice(pricePerLiter, updatedAt);
        }
        else
        {
            _fuelPrices.Add(FuelPrice.Create(fuelType, pricePerLiter, updatedAt));
        }

        LastUpdated = DateTime.UtcNow;
    }

    public void MarkAsSynced(int stationCount)
    {
        RaiseDomainEvent(new StationPricesSyncedEvent(stationCount));
    }

#pragma warning disable CS8618
    private StationPrice() : base(default!) { }
#pragma warning restore CS8618
}
