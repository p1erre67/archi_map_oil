using MediatR;
using PriceWatch.Modules.History.Application.DTOs;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.History.Application.Queries.GetStationPriceHistory;

public sealed record GetStationPriceHistoryQuery(
    string ExternalStationId,
    string? FuelType = null) : IRequest<Result<IReadOnlyList<PriceRecordDto>>>;
