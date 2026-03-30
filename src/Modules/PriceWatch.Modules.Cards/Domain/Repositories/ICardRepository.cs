using PriceWatch.Modules.Cards.Domain.Entities;

namespace PriceWatch.Modules.Cards.Domain.Repositories;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid cardId, CancellationToken cancellationToken = default);
    void Add(Card card);
    void Update(Card card);
}
