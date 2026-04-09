using PriceWatch.Modules.History.Domain.Entities;

namespace PriceWatch.Modules.History.Domain.Repositories;

public interface IPriceRecordRepository
{
    Task<IReadOnlyList<PriceRecord>> GetByStationAsync(
        string externalStationId,
        string? fuelType = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PriceRecord>> GetGlobalAveragesAsync(
        string fuelType,
        DateTime from,
        DateTime to,
        IReadOnlyList<string>? stationIds = null,
        CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<PriceRecord> records);
}
