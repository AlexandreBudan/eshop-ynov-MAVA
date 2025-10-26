using BuildingBlocks.CQRS;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

/// <summary>
/// Represents a command to update an existing order in the system.
/// </summary>
/// <remarks>
/// The command carries the essential data required to update an order, encapsulated within a single
/// instance of the <see cref="OrderDto"/> data transfer object. This command is processed by a handler
/// to complete the operation.
/// </remarks>
/// <param name="Order">
/// The <see cref="OrderDto"/> object containing the updated details of the order.
/// This includes general order information, customer details, shipping and billing addresses,
/// payment details, order status, and the associated order items.
/// </param>
/// <returns>
/// An instance of <see cref="UpdateOrderCommandResult"/> indicating the success or failure of the operation.
/// </returns>
public record UpdateOrderCommand(OrderDto Order) : ICommand<UpdateOrderCommandResult>;