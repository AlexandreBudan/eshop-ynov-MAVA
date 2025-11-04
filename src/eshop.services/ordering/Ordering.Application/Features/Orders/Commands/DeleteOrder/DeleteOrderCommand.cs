using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Commands.DeleteOrder;

/// <summary>
/// Represents a command to delete an order.
/// </summary>
/// <remarks>
/// This command is used to trigger the deletion of a specified order.
/// The <see cref="DeleteOrderCommand"/> requires a valid order identifier
/// to execute the delete operation.
/// </remarks>
public record DeleteOrderCommand(Guid OrderId) : ICommand<DeleteOrderCommandResult>;