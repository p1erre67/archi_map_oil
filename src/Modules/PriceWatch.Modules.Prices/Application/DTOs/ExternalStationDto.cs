namespace PriceWatch.Modules.Prices.Application.DTOs;

/// <summary>
/// DTO representing station data returned from the external fuel price API.
/// </summary>
public sealed record ExternalStationDto(
    string Id,
    string Name,
    string Address,
    string City,
    string PostalCode,
    double Lat,
    double Lon,
    IReadOnlyList<ExternalFuelPriceDto> FuelPrices);

/// <summary>
/// DTO representing one fuel price entry from the external API.
/// </summary>
public sealed record ExternalFuelPriceDto(
    string FuelType,
    decimal Price,
    DateTime UpdatedAt);
