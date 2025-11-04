namespace Ordering.Domain.Abstractions;

/// <summary>
/// Represents an aggregate root within the domain model, which contains domain logic and tracks domain events.
/// </summary>
/// <typeparam name="TId">The type of the unique identifier for the aggregate.</typeparam>
public class Aggregate<TId> : Entity<TId>, IAggregate<TId>
{
    /// <summary>
    /// A private collection used to manage the domain events associated with the aggregate.
    /// Domain events capture significant changes or occurrences within the domain model.
    /// This list supports operations such as adding new events and clearing them after processing.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new ();
    
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the collection of domain events tracked by the aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to be added to the aggregate's domain event list.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Insert(0, domainEvent);
    }

    /// <summary>
    /// Clears all domain events tracked by the aggregate and returns them as an array.
    /// </summary>
    /// <returns>An array containing the domain events that were cleared from the aggregate's domain event list.</returns>
    public IDomainEvent[] ClearDomainEvents()
    {
        var domainEvents = _domainEvents.ToArray();
        _domainEvents.Clear();
        return domainEvents;
    }
}