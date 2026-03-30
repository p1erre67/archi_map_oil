using MediatR;

namespace PriceWatch.SharedKernel.Domain.Events;

/// <summary>
/// Base record for domain events.
/// Using record ensures structural immutability and clean serialization.
/// Implements INotification so MediatR can publish them directly.
/// </summary>
public abstract record DomainEvent : INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
