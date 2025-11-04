namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

/// <summary>
/// Represents the result of the execution of an update order command.
/// </summary>
/// <remarks>
/// The result indicates whether the operation to update an order was successful or not,
/// providing a simple way to confirm the outcome of the command.
/// </remarks>
/// <param name="IsSuccess">
/// A boolean value where true indicates that the update operation was successful,
/// and false indicates a failure or an error occurred during the operation.
/// </param>
public record UpdateOrderCommandResult(bool IsSuccess);