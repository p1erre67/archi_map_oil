using MediatR;
using PriceWatch.Modules.Cards.Application.Interfaces;
using PriceWatch.Modules.Cards.Domain.Errors;
using PriceWatch.Modules.Cards.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Commands.BlockCard;

internal sealed class BlockCardCommandHandler : IRequestHandler<BlockCardCommand, Result>
{
    private readonly ICardRepository _repository;
    private readonly ICardsUnitOfWork _unitOfWork;

    public BlockCardCommandHandler(
        ICardRepository repository,
        ICardsUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(BlockCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _repository.GetByIdAsync(request.CardId, cancellationToken);
        if (card is null)
            return CardErrors.NotFound(request.CardId);

        var result = card.Block();
        if (result.IsFailure)
            return result.Error;

        _repository.Update(card);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
