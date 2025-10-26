using MediatR;

namespace Ordering.Domain.Abstractions;

/// <summary>
/// Represents the base interface for a domain event within the domain model.
/// </summary>
/// <remarks>
/// A domain event signifies something that has occurred within the domain that is significant
/// to the business or other parts of the application. Implementing this interface allows the event
/// to be recognized as part of the domain's event-driven architecture.
/// </remarks>
public interface IDomainEvent : INotification
{
    public Guid EventId => Guid.NewGuid();
    public DateTime OccurredOn => DateTime.Now;
    public string? EventType => GetType().AssemblyQualifiedName;
}