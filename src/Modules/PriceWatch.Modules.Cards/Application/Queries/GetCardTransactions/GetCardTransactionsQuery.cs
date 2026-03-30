using MediatR;
using PriceWatch.Modules.Cards.Application.DTOs;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Queries.GetCardTransactions;

public sealed record GetCardTransactionsQuery(
    Guid CardId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<CardTransactionDto>>>;
