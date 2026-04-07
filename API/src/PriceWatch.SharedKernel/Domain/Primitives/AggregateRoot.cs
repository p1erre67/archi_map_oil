using PriceWatch.SharedKernel.Domain.Events;

namespace PriceWatch.SharedKernel.Domain.Primitives;

/// <summary>
/// Non-generic marker interface so EF Core's ChangeTracker can detect aggregates
/// without knowing the generic TId parameter.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyList<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

/// <summary>
/// Base class for aggregate roots.
/// Aggregates are the entry point for all domain operations and own their domain events.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id) { }

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(DomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() =>
        _domainEvents.Clear();
}
