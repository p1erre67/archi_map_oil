using PriceWatch.Modules.Cards.Domain.ValueObjects;
using PriceWatch.SharedKernel.Domain.Events;

namespace PriceWatch.Modules.Cards.Domain.Events;

public sealed record FuelTransactionRecordedEvent(
    CardId CardId,
    string FuelType,
    decimal Liters,
    decimal PricePerLiter,
    decimal TotalCharged,
    string StationId) : DomainEvent;
