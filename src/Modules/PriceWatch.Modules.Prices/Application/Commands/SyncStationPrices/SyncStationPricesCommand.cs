using MediatR;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;

public sealed record SyncStationPricesCommand(
    IReadOnlyList<ExternalStationDto> Stations) : IRequest<Result>;
