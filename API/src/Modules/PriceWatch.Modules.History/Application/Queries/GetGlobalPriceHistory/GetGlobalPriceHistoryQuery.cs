using MediatR;
using PriceWatch.Modules.History.Application.DTOs;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.History.Application.Queries.GetGlobalPriceHistory;

public sealed record GetGlobalPriceHistoryQuery(
    string FuelType,
    DateTime From,
    DateTime To) : IRequest<Result<IReadOnlyList<GlobalPricePointDto>>>;
