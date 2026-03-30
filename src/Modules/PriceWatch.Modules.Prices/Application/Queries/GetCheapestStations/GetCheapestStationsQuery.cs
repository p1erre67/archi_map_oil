using MediatR;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Prices.Application.Queries.GetCheapestStations;

public sealed record GetCheapestStationsQuery(
    string FuelType,
    int Limit = 10) : IRequest<Result<IReadOnlyList<StationPriceDto>>>;
