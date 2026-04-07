using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PriceWatch.Modules.History.Application.Queries.GetGlobalPriceHistory;
using PriceWatch.Modules.History.Application.Queries.GetStationPriceHistory;
using PriceWatch.SharedKernel.Application.Interfaces;

namespace PriceWatch.Modules.History.Endpoints;

public sealed class HistoryEndpoints : IEndpoint
{
    public void MapEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/history")
            .WithTags("History");

        group.MapGet("/stations/{externalStationId}", async (
            string externalStationId,
            string? fuelType,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetStationPriceHistoryQuery(externalStationId, fuelType), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error.Code.Contains("NotFound")
                    ? Results.NotFound(result.Error.Description)
                    : Results.Problem(result.Error.Description,
                        statusCode: StatusCodes.Status500InternalServerError);
        });

        group.MapGet("/global", async (
            string fuelType,
            DateTime from,
            DateTime to,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetGlobalPriceHistoryQuery(fuelType, from, to), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error.Description,
                    statusCode: StatusCodes.Status500InternalServerError);
        });
    }
}
