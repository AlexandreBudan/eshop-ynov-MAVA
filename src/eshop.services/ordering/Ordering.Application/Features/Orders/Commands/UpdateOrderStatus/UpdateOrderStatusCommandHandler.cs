using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler(IOrderingDbContext orderingDbContext)
    : ICommandHandler<UpdateOrderStatusCommand, UpdateOrderStatusCommandResult>
{
    /// <summary>
    /// Handles the execution logic for updating an order's status.
    /// </summary>
    /// <param name="request">The command containing the order ID and new status.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>Result indicating whether the status was updated successfully.</returns>
    /// <exception cref="NotFoundException">Thrown when the order is not found.</exception>
    public async Task<UpdateOrderStatusCommandResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Of(request.OrderId);

        var order = await orderingDbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException($"Order with ID {request.OrderId} was not found.");
        }

        // Update the order status
        order.UpdateStatus(request.NewStatus);

        // Save changes to persist the update
        await orderingDbContext.SaveChangesAsync(cancellationToken);

        return new UpdateOrderStatusCommandResult(true);
    }
}
