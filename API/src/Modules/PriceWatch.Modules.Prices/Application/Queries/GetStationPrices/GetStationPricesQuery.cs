using MediatR;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Prices.Application.Queries.GetStationPrices;

public sealed record GetStationPricesQuery(string ExternalStationId) : IRequest<Result<StationPriceDto>>;
