using Ordering.Domain.Abstractions;
using Ordering.Domain.Models;

namespace Ordering.Domain.Events;

/// <summary>
/// Event raised when an order is cancelled.
/// </summary>
public record OrderCancelledEvent(Order Order) : IDomainEvent;