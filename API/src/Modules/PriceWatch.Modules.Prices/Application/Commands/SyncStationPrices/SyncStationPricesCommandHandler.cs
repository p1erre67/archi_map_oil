using MediatR;
using PriceWatch.Modules.Prices.Application.Interfaces;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;

internal sealed class SyncStationPricesCommandHandler : IRequestHandler<SyncStationPricesCommand, Result>
{
    private readonly IStationPriceRepository _repository;
    private readonly IPricesUnitOfWork _unitOfWork;

    public SyncStationPricesCommandHandler(
        IStationPriceRepository repository,
        IPricesUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SyncStationPricesCommand request, CancellationToken cancellationToken)
    {
        StationPrice? lastProcessed = null;

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
                lastProcessed = existing;
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
                lastProcessed = station;
            }
        }

        // L'aggregat leve le domain event — le DbContext le dispatche automatiquement au SaveChanges
        lastProcessed?.MarkAsSynced(request.Stations.Count);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
