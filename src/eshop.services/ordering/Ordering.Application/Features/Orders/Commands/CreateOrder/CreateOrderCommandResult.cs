namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Represents the result of the CreateOrderCommand execution.
/// </summary>
/// <remarks>
/// Contains information about the newly created order, specifically its unique identifier.
/// This result is returned after successfully executing the CreateOrderCommand,
/// which creates an order in the system with the provided details.
/// </remarks>
/// <param name="NewOrderId">
/// The unique identifier of the newly created order.
/// </param>
public record CreateOrderCommandResult(Guid NewOrderId);