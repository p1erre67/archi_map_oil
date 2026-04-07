using MediatR;

namespace PriceWatch.SharedKernel.Application.Events;

/// <summary>
/// Marker interface for integration events.
/// Integration events cross module boundaries (unlike DomainEvents which stay internal).
/// Published via MediatR so consuming modules can react without direct references.
/// </summary>
public interface IIntegrationEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}
