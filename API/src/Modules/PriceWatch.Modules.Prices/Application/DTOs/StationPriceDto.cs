namespace PriceWatch.Modules.Prices.Application.DTOs;

public sealed record StationPriceDto(
    Guid Id,
    string ExternalStationId,
    string StationName,
    string? BrandName,
    string Address,
    string City,
    string PostalCode,
    double Latitude,
    double Longitude,
    DateTime LastUpdated,
    IReadOnlyList<FuelPriceDto> FuelPrices);

public sealed record FuelPriceDto(
    string FuelType,
    decimal PricePerLiter,
    DateTime UpdatedAt);
