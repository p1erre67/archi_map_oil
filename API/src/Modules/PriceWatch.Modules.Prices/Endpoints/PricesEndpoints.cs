using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;
using PriceWatch.Modules.Prices.Application.Interfaces;
using PriceWatch.Modules.Prices.Application.Queries.GetCheapestStations;
using PriceWatch.Modules.Prices.Application.Queries.GetNearbyStations;
using PriceWatch.Modules.Prices.Application.Queries.GetStationPrices;
using PriceWatch.SharedKernel.Application.Interfaces;

namespace PriceWatch.Modules.Prices.Endpoints;

public sealed class PricesEndpoints : IEndpoint
{
    public void MapEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/prices")
            .WithTags("Prices");

        group.MapGet("/stations/{externalStationId}", async (
            string externalStationId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetStationPricesQuery(externalStationId), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error.Code.Contains("NotFound")
                    ? Results.NotFound(result.Error.Description)
                    : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status500InternalServerError);
        });

        group.MapGet("/cheapest", async (
            string fuelType,
            int limit,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetCheapestStationsQuery(fuelType, limit <= 0 ? 10 : limit);
            var result = await sender.Send(query, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status500InternalServerError);
        });

        group.MapGet("/nearby", async (
            double latitude,
            double longitude,
            int radiusKm,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetNearbyStationsQuery(latitude, longitude, radiusKm <= 0 ? 10 : radiusKm);
            var result = await sender.Send(query, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status500InternalServerError);
        });

        group.MapPost("/sync", async (
            SyncRequest req,
            ISender sender,
            IFuelPriceApiClient apiClient,
            CancellationToken ct) =>
        {
            var stations = await apiClient.GetStationsAsync(
                req.Latitude,
                req.Longitude,
                req.RadiusKm,
                ct);

            var result = await sender.Send(new SyncStationPricesCommand(stations), ct);
            return result.IsSuccess
                ? Results.Ok(new { message = $"Synced {stations.Count} station(s)." })
                : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status500InternalServerError);
        });
    }

    private sealed record SyncRequest(double Latitude, double Longitude, int RadiusKm);
}
