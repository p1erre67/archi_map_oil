using MediatR;
using PriceWatch.Modules.Cards.Application.Interfaces;
using PriceWatch.Modules.Cards.Domain.Errors;
using PriceWatch.Modules.Cards.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Commands.RecordFuelTransaction;

internal sealed class RecordFuelTransactionCommandHandler : IRequestHandler<RecordFuelTransactionCommand, Result>
{
    private readonly ICardRepository _repository;
    private readonly ICardsUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public RecordFuelTransactionCommandHandler(
        ICardRepository repository,
        ICardsUnitOfWork unitOfWork,
        IPublisher publisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Result> Handle(RecordFuelTransactionCommand request, CancellationToken cancellationToken)
    {
        var card = await _repository.GetByIdAsync(request.CardId, cancellationToken);
        if (card is null)
            return CardErrors.NotFound(request.CardId);

        var result = card.RecordFuelTransaction(
            request.FuelType,
            request.Liters,
            request.PricePerLiter,
            request.StationId);

        if (result.IsFailure)
            return result.Error;

        _repository.Update(card);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in card.DomainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);
        card.ClearDomainEvents();

        return Result.Success();
    }
}
