namespace PriceWatch.Modules.History.Application.Interfaces;

public interface IHistoryUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
