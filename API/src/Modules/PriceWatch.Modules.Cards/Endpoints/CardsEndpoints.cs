using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PriceWatch.Modules.Cards.Application.Commands.BlockCard;
using PriceWatch.Modules.Cards.Application.Commands.CreateCard;
using PriceWatch.Modules.Cards.Application.Commands.RecordFuelTransaction;
using PriceWatch.Modules.Cards.Application.Commands.TopUpCard;
using PriceWatch.Modules.Cards.Application.Queries.GetCard;
using PriceWatch.Modules.Cards.Application.Queries.GetCardTransactions;
using PriceWatch.SharedKernel.Application.Interfaces;

namespace PriceWatch.Modules.Cards.Endpoints;

public sealed class CardsEndpoints : IEndpoint
{
    public void MapEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/cards")
            .WithTags("Cards");

        group.MapPost("/", async (CreateCardCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/cards/{result.Value}", new { id = result.Value })
                : result.Error.Code.Contains("Validation")
                    ? Results.UnprocessableEntity(result.Error.Description)
                    : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status500InternalServerError);
        });

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCardQuery(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error.Code.Contains("NotFound")
                    ? Results.NotFound(result.Error.Description)
                    : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status500InternalServerError);
        });

        group.MapPost("/{id:guid}/topup", async (Guid id, TopUpRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new TopUpCardCommand(id, req.Amount), ct);
            return result.IsSuccess
                ? Results.Ok()
                : result.Error.Code.Contains("NotFound")
                    ? Results.NotFound(result.Error.Description)
                    : result.Error.Code.Contains("Validation")
                        ? Results.UnprocessableEntity(result.Error.Description)
                        : Results.Conflict(result.Error.Description);
        });

        group.MapPost("/{id:guid}/transactions", async (Guid id, FuelTransactionRequest req, ISender sender, CancellationToken ct) =>
        {
            var command = new RecordFuelTransactionCommand(id, req.FuelType, req.Liters, req.PricePerLiter, req.StationId);
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok()
                : result.Error.Code.Contains("NotFound")
                    ? Results.NotFound(result.Error.Description)
                    : result.Error.Code.Contains("Validation")
                        ? Results.UnprocessableEntity(result.Error.Description)
                        : Results.Conflict(result.Error.Description);
        });

        group.MapGet("/{id:guid}/transactions", async (
            Guid id,
            int page,
            int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetCardTransactionsQuery(id, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize);
            var result = await sender.Send(query, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error.Code.Contains("NotFound")
                    ? Results.NotFound(result.Error.Description)
                    : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status500InternalServerError);
        });

        group.MapPost("/{id:guid}/block", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new BlockCardCommand(id), ct);
            return result.IsSuccess
                ? Results.Ok()
                : result.Error.Code.Contains("NotFound")
                    ? Results.NotFound(result.Error.Description)
                    : result.Error.Code.Contains("Conflict")
                        ? Results.Conflict(result.Error.Description)
                        : Results.Problem(result.Error.Description, statusCode: StatusCodes.Status500InternalServerError);
        });
    }

    private sealed record TopUpRequest(decimal Amount);
    private sealed record FuelTransactionRequest(string FuelType, decimal Liters, decimal PricePerLiter, string StationId);
}
