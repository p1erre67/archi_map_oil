using PriceWatch.Modules.Cards.Domain.ValueObjects;
using PriceWatch.SharedKernel.Domain.Events;

namespace PriceWatch.Modules.Cards.Domain.Events;

public sealed record CardToppedUpEvent(CardId CardId, decimal Amount, decimal NewBalance) : DomainEvent;
