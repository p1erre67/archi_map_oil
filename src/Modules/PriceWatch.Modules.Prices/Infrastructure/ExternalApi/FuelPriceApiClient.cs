using System.Net.Http.Json;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.Modules.Prices.Application.Interfaces;

namespace PriceWatch.Modules.Prices.Infrastructure.ExternalApi;

/// <summary>
/// HTTP client implementation for the French government fuel price API.
/// Calls GET /stations/around/{lat}/{lon}?radius={km}
/// </summary>
internal sealed class FuelPriceApiClient : IFuelPriceApiClient
{
    private readonly HttpClient _httpClient;

    public FuelPriceApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ExternalStationDto>> GetStationsAsync(
        double lat,
        double lon,
        int radiusKm,
        CancellationToken cancellationToken = default)
    {
        var url = $"/stations/around/{lat}/{lon}?radius={radiusKm}";

        var response = await _httpClient.GetFromJsonAsync<List<ApiStationResponse>>(url, cancellationToken);

        if (response is null)
            return [];

        return response.Select(s => new ExternalStationDto(
            s.Id,
            s.Name,
            s.Address,
            s.City,
            s.PostalCode,
            s.Lat,
            s.Lon,
            s.Fuels.Select(f => new ExternalFuelPriceDto(
                f.FuelType,
                f.Price,
                f.UpdatedAt)).ToList())).ToList();
    }

    // ── Private API response shapes ──────────────────────────────────────────

    private sealed record ApiStationResponse(
        string Id,
        string Name,
        string Address,
        string City,
        string PostalCode,
        double Lat,
        double Lon,
        List<ApiFuelResponse> Fuels);

    private sealed record ApiFuelResponse(
        string FuelType,
        decimal Price,
        DateTime UpdatedAt);
}
