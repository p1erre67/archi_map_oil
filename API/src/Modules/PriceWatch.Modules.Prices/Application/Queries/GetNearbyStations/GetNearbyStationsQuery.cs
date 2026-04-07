using MediatR;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Prices.Application.Queries.GetNearbyStations;

public sealed record GetNearbyStationsQuery(
    double Latitude,
    double Longitude,
    double RadiusKm) : IRequest<Result<IReadOnlyList<StationPriceDto>>>;
