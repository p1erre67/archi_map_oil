using PriceWatch.Modules.Prices.Domain.Entities;

namespace PriceWatch.Modules.Prices.Domain.Repositories;

public interface IBrandRepository
{
    Task<Brand?> GetByExternalIdAsync(int externalId, CancellationToken cancellationToken = default);
    void Add(Brand brand);
}
