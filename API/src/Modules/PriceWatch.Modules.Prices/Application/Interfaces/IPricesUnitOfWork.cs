namespace PriceWatch.Modules.Prices.Application.Interfaces;

public interface IPricesUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
