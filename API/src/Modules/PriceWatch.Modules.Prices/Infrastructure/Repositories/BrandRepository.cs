using Microsoft.EntityFrameworkCore;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Repositories;
using PriceWatch.Modules.Prices.Infrastructure.Persistence;

namespace PriceWatch.Modules.Prices.Infrastructure.Repositories;

internal sealed class BrandRepository : IBrandRepository
{
    private readonly PricesDbContext _context;

    public BrandRepository(PricesDbContext context)
    {
        _context = context;
    }

    public async Task<Brand?> GetByExternalIdAsync(
        int externalId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Brands
            .FirstOrDefaultAsync(b => b.ExternalId == externalId, cancellationToken);
    }

    public void Add(Brand brand)
        => _context.Brands.Add(brand);
}
