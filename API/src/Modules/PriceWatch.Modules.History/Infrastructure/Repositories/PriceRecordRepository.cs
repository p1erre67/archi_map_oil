using Microsoft.EntityFrameworkCore;
using PriceWatch.Modules.History.Domain.Entities;
using PriceWatch.Modules.History.Domain.Repositories;
using PriceWatch.Modules.History.Infrastructure.Persistence;

namespace PriceWatch.Modules.History.Infrastructure.Repositories;

internal sealed class PriceRecordRepository : IPriceRecordRepository
{
    private readonly HistoryDbContext _context;

    public PriceRecordRepository(HistoryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PriceRecord>> GetByStationAsync(
        string externalStationId,
        string? fuelType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PriceRecords
            .Where(r => r.ExternalStationId == externalStationId);

        if (fuelType is not null)
            query = query.Where(r => r.FuelType == fuelType);

        return await query
            .OrderBy(r => r.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PriceRecord>> GetGlobalAveragesAsync(
        string fuelType,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await _context.PriceRecords
            .Where(r => r.FuelType == fuelType && r.RecordedAt >= from && r.RecordedAt <= to)
            .OrderBy(r => r.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public void AddRange(IEnumerable<PriceRecord> records)
        => _context.PriceRecords.AddRange(records);
}
