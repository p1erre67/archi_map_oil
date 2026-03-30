using Microsoft.EntityFrameworkCore;
using PriceWatch.Modules.Cards.Domain.Entities;
using PriceWatch.Modules.Cards.Domain.Repositories;
using PriceWatch.Modules.Cards.Infrastructure.Persistence;

namespace PriceWatch.Modules.Cards.Infrastructure.Repositories;

internal sealed class CardRepository : ICardRepository
{
    private readonly CardsDbContext _context;

    public CardRepository(CardsDbContext context)
    {
        _context = context;
    }

    public async Task<Card?> GetByIdAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        return await _context.Cards
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id.Value == cardId, cancellationToken);
    }

    public void Add(Card card) => _context.Cards.Add(card);

    public void Update(Card card) => _context.Cards.Update(card);
}
