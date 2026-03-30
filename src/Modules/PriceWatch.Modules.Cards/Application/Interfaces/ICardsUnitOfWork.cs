namespace PriceWatch.Modules.Cards.Application.Interfaces;

public interface ICardsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
