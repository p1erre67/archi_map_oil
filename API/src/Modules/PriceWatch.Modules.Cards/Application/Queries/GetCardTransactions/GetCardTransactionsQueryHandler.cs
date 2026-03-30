using MediatR;
using PriceWatch.Modules.Cards.Application.DTOs;
using PriceWatch.Modules.Cards.Domain.Errors;
using PriceWatch.Modules.Cards.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Queries.GetCardTransactions;

internal sealed class GetCardTransactionsQueryHandler
    : IRequestHandler<GetCardTransactionsQuery, Result<IReadOnlyList<CardTransactionDto>>>
{
    private readonly ICardRepository _repository;

    public GetCardTransactionsQueryHandler(ICardRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CardTransactionDto>>> Handle(
        GetCardTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var card = await _repository.GetByIdAsync(request.CardId, cancellationToken);
        if (card is null)
            return CardErrors.NotFound(request.CardId);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var transactions = card.Transactions
            .OrderByDescending(t => t.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new CardTransactionDto(
                t.TransactionId,
                t.Type.ToString(),
                t.AmountCharged,
                t.LitersFueled,
                t.FuelType,
                t.StationId,
                t.OccurredAt))
            .ToList();

        return transactions;
    }
}
