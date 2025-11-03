namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Represents the result of an update order status command.
/// </summary>
/// <param name="IsSuccess">Indicates whether the status update was successful.</param>
public record UpdateOrderStatusCommandResult(bool IsSuccess);
