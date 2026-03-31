using PriceWatch.SharedKernel.Domain.Events;

namespace PriceWatch.Modules.Prices.Domain.Events;

public sealed record StationPricesSyncedEvent(int StationCount) : DomainEvent;
