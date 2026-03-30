using PriceWatch.Modules.Cards.Domain.ValueObjects;
using PriceWatch.SharedKernel.Domain.Events;

namespace PriceWatch.Modules.Cards.Domain.Events;

public sealed record CardCreatedEvent(CardId CardId, CardNumber CardNumber, string HolderName) : DomainEvent;
