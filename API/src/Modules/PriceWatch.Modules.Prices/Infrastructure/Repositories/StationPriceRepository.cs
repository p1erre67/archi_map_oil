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

    public async Task<IReadOnlyList<StationPrice>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.StationPrices
            .Include(s => s.FuelPrices)
            .ToListAsync(cancellationToken);
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

    public async Task<IReadOnlyList<StationPrice>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusKm,
        CancellationToken cancellationToken = default)
    {
        const double earthRadiusKm = 6371.0;
        var latRad = latitude * Math.PI / 180.0;
        var lonRad = longitude * Math.PI / 180.0;

        return await _context.StationPrices
            .Include(s => s.FuelPrices)
            .Where(s =>
                earthRadiusKm * 2.0 * Math.Asin(Math.Sqrt(
                    Math.Pow(Math.Sin((s.Latitude * Math.PI / 180.0 - latRad) / 2.0), 2) +
                    Math.Cos(latRad) * Math.Cos(s.Latitude * Math.PI / 180.0) *
                    Math.Pow(Math.Sin((s.Longitude * Math.PI / 180.0 - lonRad) / 2.0), 2)
                )) <= radiusKm)
            .OrderBy(s =>
                earthRadiusKm * 2.0 * Math.Asin(Math.Sqrt(
                    Math.Pow(Math.Sin((s.Latitude * Math.PI / 180.0 - latRad) / 2.0), 2) +
                    Math.Cos(latRad) * Math.Cos(s.Latitude * Math.PI / 180.0) *
                    Math.Pow(Math.Sin((s.Longitude * Math.PI / 180.0 - lonRad) / 2.0), 2)
                )))
            .ToListAsync(cancellationToken);
    }

    public void Add(StationPrice stationPrice)
        => _context.StationPrices.Add(stationPrice);

    public void Update(StationPrice stationPrice)
        => _context.StationPrices.Update(stationPrice);
}
