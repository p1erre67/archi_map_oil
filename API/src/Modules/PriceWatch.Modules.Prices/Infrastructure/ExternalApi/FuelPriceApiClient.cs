using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.Modules.Prices.Application.Interfaces;

namespace PriceWatch.Modules.Prices.Infrastructure.ExternalApi;

/// <summary>
/// HTTP client implementation for the French government fuel price API.
/// Calls GET /stations/around/{lat},{lon}?radius={km}
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
        var latStr = lat.ToString(CultureInfo.InvariantCulture);
        var lonStr = lon.ToString(CultureInfo.InvariantCulture);
        var url = $"/stations/around/{latStr},{lonStr}?responseFields=Fuels,Price";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Range", $"m=0-{radiusKm * 1000}");

        var httpResponse = await _httpClient.SendAsync(request, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<List<ApiStationResponse>>(cancellationToken);

        if (response is null)
            return [];

        return response.Select(s => new ExternalStationDto(
            s.Id.ToString(),
            s.Name ?? "",
            s.Address?.StreetLine ?? "",
            ParseCity(s.Address?.CityLine),
            ParsePostalCode(s.Address?.CityLine),
            s.Coordinates?.Latitude ?? 0,
            s.Coordinates?.Longitude ?? 0,
            s.Fuels?.Select(f => new ExternalFuelPriceDto(
                f.ShortName ?? f.Name ?? "",
                f.PriceInfo?.Value ?? 0,
                EnsureUtc(f.Update?.Value ?? DateTime.UtcNow))).ToList()
            ?? [],
            s.Brand?.Id,
            s.Brand?.Name,
            s.Brand?.ShortName,
            s.Brand?.NbStations)).ToList();
    }

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static string ParsePostalCode(string? cityLine) =>
        cityLine is not null && cityLine.Length >= 5 ? cityLine[..5] : "";

    private static string ParseCity(string? cityLine) =>
        cityLine is not null && cityLine.Length > 6 ? cityLine[6..] : "";

    // ── Private API response shapes (matches actual API structure) ────────────

    private sealed record ApiStationResponse(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("Address")] ApiAddress? Address,
        [property: JsonPropertyName("Coordinates")] ApiCoordinates? Coordinates,
        [property: JsonPropertyName("Fuels")] List<ApiFuelResponse>? Fuels,
        [property: JsonPropertyName("Brand")] ApiBrand? Brand);

    private sealed record ApiAddress(
        [property: JsonPropertyName("street_line")] string? StreetLine,
        [property: JsonPropertyName("city_line")] string? CityLine);

    private sealed record ApiCoordinates(
        [property: JsonPropertyName("latitude")] double Latitude,
        [property: JsonPropertyName("longitude")] double Longitude);

    private sealed record ApiFuelResponse(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("shortName")] string? ShortName,
        [property: JsonPropertyName("Price")] ApiFuelPrice? PriceInfo,
        [property: JsonPropertyName("Update")] ApiTimestamp? Update);

    private sealed record ApiFuelPrice(
        [property: JsonPropertyName("value")] decimal Value);

    private sealed record ApiTimestamp(
        [property: JsonPropertyName("value")] DateTime Value);

    private sealed record ApiBrand(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("shortName")] string? ShortName,
        [property: JsonPropertyName("nb_stations")] int? NbStations);
}
