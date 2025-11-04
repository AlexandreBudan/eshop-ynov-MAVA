using BuildingBlocks.CQRS;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Represents a command to create an order in the ordering system.
/// </summary>
/// <remarks>
/// This command encapsulates the details required to create a new order,
/// including customer and order-specific information such as the shipping
/// and billing addresses, payment details, order items, and the order status.
/// The associated validator, <see cref="CreateOrderCommandValidator"/>, ensures
/// that the command contains all the required information and adheres to the
/// defined validation rules.
/// </remarks>
/// <param name="Order">
/// The order details encapsulated in an <see cref="OrderDto"/> object. It includes
/// information such as the customer ID, order name, addresses, payment details,
/// status, and items included in the order.
/// </param>
/// <returns>
/// A <see cref="CreateOrderCommandResult"/> that contains the unique identifier
/// of the newly created order.
/// </returns>
public record CreateOrderCommand(OrderDto Order) : ICommand<CreateOrderCommandResult>;