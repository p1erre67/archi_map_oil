using MediatR;
using PriceWatch.Modules.Cards.Application.DTOs;
using PriceWatch.Modules.Cards.Domain.Errors;
using PriceWatch.Modules.Cards.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Queries.GetCard;

internal sealed class GetCardQueryHandler : IRequestHandler<GetCardQuery, Result<CardDto>>
{
    private readonly ICardRepository _repository;

    public GetCardQueryHandler(ICardRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CardDto>> Handle(GetCardQuery request, CancellationToken cancellationToken)
    {
        var card = await _repository.GetByIdAsync(request.CardId, cancellationToken);
        if (card is null)
            return CardErrors.NotFound(request.CardId);

        var dto = new CardDto(
            card.Id.Value,
            card.CardNumber.Value,
            card.HolderName,
            card.Balance,
            card.Status.ToString(),
            card.CreatedAt);

        return dto;
    }
}
