using Ordering.Domain.Abstractions;
using Ordering.Domain.Models;

namespace Ordering.Domain.Events;

/// <summary>
/// Represents a domain event used to signify that an Order has been updated.
/// </summary>
/// <remarks>
/// This event is triggered when an existing <see cref="Order"/> object undergoes an update
/// to reflect changes in its attributes or properties, such as order name, shipping address,
/// billing address, payment details, or order status.
/// </remarks>
/// <param name="Order">
/// The <see cref="Order"/> instance that has been updated.
/// Encapsulates all relevant data about the order, including its items, customer,
/// and associated metadata.
/// </param>
public record OrderUpdatedEvent(Order Order) : IDomainEvent;