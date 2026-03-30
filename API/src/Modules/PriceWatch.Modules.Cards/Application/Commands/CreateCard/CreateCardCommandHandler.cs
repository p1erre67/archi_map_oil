using MediatR;
using PriceWatch.Modules.Cards.Application.Interfaces;
using PriceWatch.Modules.Cards.Domain.Entities;
using PriceWatch.Modules.Cards.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Commands.CreateCard;

internal sealed class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, Result<Guid>>
{
    private readonly ICardRepository _repository;
    private readonly ICardsUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public CreateCardCommandHandler(
        ICardRepository repository,
        ICardsUnitOfWork unitOfWork,
        IPublisher publisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(CreateCardCommand request, CancellationToken cancellationToken)
    {
        var result = Card.Create(request.HolderName);
        if (result.IsFailure)
            return result.Error;

        var card = result.Value;
        _repository.Add(card);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in card.DomainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);
        card.ClearDomainEvents();

        return card.Id.Value;
    }
}
