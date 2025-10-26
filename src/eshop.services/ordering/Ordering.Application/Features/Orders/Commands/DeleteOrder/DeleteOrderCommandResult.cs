namespace Ordering.Application.Features.Orders.Commands.DeleteOrder;

/// <summary>
/// Represents the result of a delete order command operation.
/// </summary>
public record DeleteOrderCommandResult(bool IsSuccess);