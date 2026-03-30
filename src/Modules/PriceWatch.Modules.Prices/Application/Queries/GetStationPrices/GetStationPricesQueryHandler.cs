using MediatR;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.Modules.Prices.Domain.Errors;
using PriceWatch.Modules.Prices.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Prices.Application.Queries.GetStationPrices;

internal sealed class GetStationPricesQueryHandler
    : IRequestHandler<GetStationPricesQuery, Result<StationPriceDto>>
{
    private readonly IStationPriceRepository _repository;

    public GetStationPricesQueryHandler(IStationPriceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<StationPriceDto>> Handle(
        GetStationPricesQuery request,
        CancellationToken cancellationToken)
    {
        var station = await _repository.GetByExternalIdAsync(request.ExternalStationId, cancellationToken);

        if (station is null)
            return PricesErrors.Station.NotFound(request.ExternalStationId);

        var dto = new StationPriceDto(
            station.Id.Value,
            station.ExternalStationId,
            station.StationName,
            station.Address,
            station.City,
            station.PostalCode,
            station.Latitude,
            station.Longitude,
            station.LastUpdated,
            station.FuelPrices.Select(fp => new FuelPriceDto(
                fp.FuelType,
                fp.PricePerLiter,
                fp.UpdatedAt)).ToList());

        return dto;
    }
}
