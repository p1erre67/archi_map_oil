using MediatR;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.Modules.Prices.Application.Interfaces;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;

internal sealed class SyncStationPricesCommandHandler : IRequestHandler<SyncStationPricesCommand, Result>
{
    private readonly IStationPriceRepository _repository;
    private readonly IBrandRepository _brandRepository;
    private readonly IPricesUnitOfWork _unitOfWork;

    public SyncStationPricesCommandHandler(
        IStationPriceRepository repository,
        IBrandRepository brandRepository,
        IPricesUnitOfWork unitOfWork)
    {
        _repository = repository;
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SyncStationPricesCommand request, CancellationToken cancellationToken)
    {
        StationPrice? lastProcessed = null;
        var brandCache = new Dictionary<int, Brand>();

        foreach (var stationDto in request.Stations)
        {
            var brandId = await UpsertBrandAsync(stationDto, brandCache, cancellationToken);

            var existing = await _repository.GetByExternalIdAsync(stationDto.Id, cancellationToken);

            if (existing is not null)
            {
                existing.UpdateInfo(
                    stationDto.Name,
                    stationDto.Address,
                    stationDto.City,
                    stationDto.PostalCode,
                    stationDto.Lat,
                    stationDto.Lon,
                    brandId);

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
                    stationDto.Lon,
                    brandId);

                foreach (var fuelPrice in stationDto.FuelPrices)
                {
                    station.UpsertFuelPrice(fuelPrice.FuelType, fuelPrice.Price, fuelPrice.UpdatedAt);
                }

                _repository.Add(station);
                lastProcessed = station;
            }
        }

        lastProcessed?.MarkAsSynced(request.Stations.Count);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<int?> UpsertBrandAsync(ExternalStationDto stationDto, Dictionary<int, Brand> cache, CancellationToken ct)
    {
        if (stationDto.BrandId is null)
            return null;

        var externalId = stationDto.BrandId.Value;

        var nbStations = stationDto.BrandNbStations ?? 0;

        if (cache.TryGetValue(externalId, out var cached))
        {
            cached.UpdateInfo(stationDto.BrandName ?? "", stationDto.BrandShortName ?? "", nbStations);
            return cached.ExternalId;
        }

        var brand = await _brandRepository.GetByExternalIdAsync(externalId, ct);

        if (brand is not null)
        {
            brand.UpdateInfo(stationDto.BrandName ?? "", stationDto.BrandShortName ?? "", nbStations);
            cache[externalId] = brand;
            return brand.ExternalId;
        }

        brand = Brand.Create(externalId, stationDto.BrandName ?? "", stationDto.BrandShortName ?? "", nbStations);
        _brandRepository.Add(brand);
        cache[externalId] = brand;
        return brand.ExternalId;
    }
}
