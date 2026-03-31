using MediatR;
using PriceWatch.Modules.Prices.Application.Interfaces;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Events;
using PriceWatch.Modules.Prices.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;

internal sealed class SyncStationPricesCommandHandler : IRequestHandler<SyncStationPricesCommand, Result>
{
    private readonly IStationPriceRepository _repository;
    private readonly IPricesUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public SyncStationPricesCommandHandler(
        IStationPriceRepository repository,
        IPricesUnitOfWork unitOfWork,
        IPublisher publisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Result> Handle(SyncStationPricesCommand request, CancellationToken cancellationToken)
    {
        foreach (var stationDto in request.Stations)
        {
            var existing = await _repository.GetByExternalIdAsync(stationDto.Id, cancellationToken);

            if (existing is not null)
            {
                existing.UpdateInfo(
                    stationDto.Name,
                    stationDto.Address,
                    stationDto.City,
                    stationDto.PostalCode,
                    stationDto.Lat,
                    stationDto.Lon);

                foreach (var fuelPrice in stationDto.FuelPrices)
                {
                    existing.UpsertFuelPrice(fuelPrice.FuelType, fuelPrice.Price, fuelPrice.UpdatedAt);
                }

                _repository.Update(existing);
            }
            else
            {
                var station = StationPrice.Create(
                    stationDto.Id,
                    stationDto.Name,
                    stationDto.Address,
                    stationDto.City,
                    stationDto.PostalCode,
                    stationDto.Lat,
                    stationDto.Lon);

                foreach (var fuelPrice in stationDto.FuelPrices)
                {
                    station.UpsertFuelPrice(fuelPrice.FuelType, fuelPrice.Price, fuelPrice.UpdatedAt);
                }

                _repository.Add(station);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new StationPricesSyncedEvent(request.Stations.Count), cancellationToken);

        return Result.Success();
    }
}
