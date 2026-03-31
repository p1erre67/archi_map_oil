using PriceWatch.Modules.Prices.Domain.Entities;

namespace PriceWatch.Modules.Prices.Domain.Repositories;

public interface IStationPriceRepository
{
    Task<StationPrice?> GetByExternalIdAsync(string externalStationId, CancellationToken cancellationToken = default);
    Task<StationPrice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StationPrice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StationPrice>> GetCheapestByFuelTypeAsync(string fuelType, int limit, CancellationToken cancellationToken = default);
    void Add(StationPrice stationPrice);
    void Update(StationPrice stationPrice);
}
