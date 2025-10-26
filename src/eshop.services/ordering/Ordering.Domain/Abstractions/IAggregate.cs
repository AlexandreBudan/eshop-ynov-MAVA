using System.Collections.ObjectModel;

namespace Ordering.Domain.Abstractions;

/// <summary>
/// Represents an aggregate within the domain model that combines entity behavior and manages domain events.
/// </summary>
/// <typeparam name="T">The type of the unique identifier for the aggregate.</typeparam>
public interface IAggregate<T> : IAggregate, IEntity<T>
{
}

/// <summary>
/// Defines a contract for an aggregate, which serves as a root for a cluster of domain entities
/// and manages domain events within the domain model.
/// </summary>
public interface IAggregate : IEntity
{
    public IReadOnlyList<IDomainEvent> DomainEvents { get; }
    
    public void AddDomainEvent(IDomainEvent domainEvent);
    
    public IDomainEvent[] ClearDomainEvents();
}