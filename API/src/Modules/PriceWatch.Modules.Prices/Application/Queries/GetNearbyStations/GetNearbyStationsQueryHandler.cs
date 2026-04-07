using MediatR;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.Modules.Prices.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Prices.Application.Queries.GetNearbyStations;

internal sealed class GetNearbyStationsQueryHandler
    : IRequestHandler<GetNearbyStationsQuery, Result<IReadOnlyList<StationPriceDto>>>
{
    private readonly IStationPriceRepository _repository;

    public GetNearbyStationsQueryHandler(IStationPriceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<StationPriceDto>>> Handle(
        GetNearbyStationsQuery request,
        CancellationToken cancellationToken)
    {
        var stations = await _repository.GetNearbyAsync(
            request.Latitude,
            request.Longitude,
            request.RadiusKm,
            cancellationToken);

        var dtos = stations.Select(s => new StationPriceDto(
            s.Id.Value,
            s.ExternalStationId,
            s.StationName,
            s.Address,
            s.City,
            s.PostalCode,
            s.Latitude,
            s.Longitude,
            s.LastUpdated,
            s.FuelPrices.Select(fp => new FuelPriceDto(
                fp.FuelType,
                fp.PricePerLiter,
                fp.UpdatedAt)).ToList())).ToList();

        return dtos;
    }
}
