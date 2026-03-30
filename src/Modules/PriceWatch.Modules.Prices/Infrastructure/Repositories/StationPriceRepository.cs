using Microsoft.EntityFrameworkCore;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Repositories;
using PriceWatch.Modules.Prices.Infrastructure.Persistence;

namespace PriceWatch.Modules.Prices.Infrastructure.Repositories;

internal sealed class StationPriceRepository : IStationPriceRepository
{
    private readonly PricesDbContext _context;

    public StationPriceRepository(PricesDbContext context)
    {
        _context = context;
    }

    public async Task<StationPrice?> GetByExternalIdAsync(
        string externalStationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StationPrices
            .Include(s => s.FuelPrices)
            .FirstOrDefaultAsync(s => s.ExternalStationId == externalStationId, cancellationToken);
    }

    public async Task<StationPrice?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.StationPrices
            .Include(s => s.FuelPrices)
            .FirstOrDefaultAsync(s => s.Id.Value == id, cancellationToken);
    }

    public async Task<IReadOnlyList<StationPrice>> GetCheapestByFuelTypeAsync(
        string fuelType,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await _context.StationPrices
            .Include(s => s.FuelPrices)
            .Where(s => s.FuelPrices.Any(fp => fp.FuelType == fuelType))
            .OrderBy(s => s.FuelPrices
                .Where(fp => fp.FuelType == fuelType)
                .Select(fp => fp.PricePerLiter)
                .Min())
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public void Add(StationPrice stationPrice)
        => _context.StationPrices.Add(stationPrice);

    public void Update(StationPrice stationPrice)
        => _context.StationPrices.Update(stationPrice);
}
