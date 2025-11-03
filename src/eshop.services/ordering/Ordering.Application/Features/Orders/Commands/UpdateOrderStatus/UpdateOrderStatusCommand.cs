using BuildingBlocks.CQRS;
using Ordering.Domain.Enums;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Represents a command to update the status of an existing order.
/// </summary>
/// <param name="OrderId">The unique identifier of the order to update.</param>
/// <param name="NewStatus">The new status to set for the order.</param>
/// <returns>Result indicating success or failure of the operation.</returns>
public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus)
    : ICommand<UpdateOrderStatusCommandResult>;
