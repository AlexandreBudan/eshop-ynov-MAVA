using Ordering.Domain.Abstractions;
using Ordering.Domain.Models;

namespace Ordering.Domain.Events;

/// <summary>
/// Represents a domain event that occurs when a new order is created.
/// </summary>
public record OrderCreatedEvent(Order Order) : IDomainEvent;