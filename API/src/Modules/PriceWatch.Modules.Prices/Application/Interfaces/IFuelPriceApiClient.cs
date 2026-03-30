using PriceWatch.Modules.Prices.Application.DTOs;

namespace PriceWatch.Modules.Prices.Application.Interfaces;

/// <summary>
/// Contract for the external fuel price API client.
/// Implemented in Infrastructure; can be mocked in tests.
/// </summary>
public interface IFuelPriceApiClient
{
    Task<IReadOnlyList<ExternalStationDto>> GetStationsAsync(
        double lat,
        double lon,
        int radiusKm,
        CancellationToken cancellationToken = default);
}
