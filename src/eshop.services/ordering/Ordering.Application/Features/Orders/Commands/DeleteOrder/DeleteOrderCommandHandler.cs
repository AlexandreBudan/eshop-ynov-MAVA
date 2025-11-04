using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Exceptions;
using Ordering.Domain.ValueObjects;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandHandler(IOrderingDbContext orderingDbContext) : ICommandHandler<DeleteOrderCommand, DeleteOrderCommandResult>
{
    /// <summary>
    /// Handles the operation for deleting an order based on the provided command.
    /// </summary>
    /// <param name="request">The command containing the order identifier to delete.</param>
    /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DeleteOrderCommandResult"/> containing the result of the delete operation.</returns>
    /// <exception cref="OrderNotFoundException">
    /// Thrown when an order with the specified identifier is not found.
    /// </exception>
    public async Task<DeleteOrderCommandResult> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Of(request.OrderId);
        var order = await orderingDbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        orderingDbContext.Orders.Remove(order);
        await orderingDbContext.SaveChangesAsync(cancellationToken);

        return new DeleteOrderCommandResult(true);
    }
}