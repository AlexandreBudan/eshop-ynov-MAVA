using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

/// <summary>
/// Handles the update order command, allowing the modification of an existing order in the system.
/// This handler retrieves the specified order, updates it with new values, and persists the changes
/// to the database. If the order does not exist, an exception is thrown.
/// </summary>
public class UpdateOrderCommandHandler(
    IOrderingDbContext orderingDbContext,
    ILogger<UpdateOrderCommandHandler> logger) : ICommandHandler<UpdateOrderCommand, UpdateOrderCommandResult>
{
    public async Task<UpdateOrderCommandResult> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating order {OrderId} for customer {CustomerId}",
            request.Order.Id, request.Order.CustomerId);

        var orderId = OrderId.Of(request.Order.Id);
        var existingOrder = await orderingDbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (existingOrder == null)
        {
            logger.LogError("Order {OrderId} not found", request.Order.Id);
            throw new NotFoundException(nameof(existingOrder), request.Order.Id);
        }

        UpdateOrderCommandMapper.UpdateOrderWithNewValues(existingOrder, request.Order);

        await orderingDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order {OrderId} updated successfully. New status: {OrderStatus}",
            existingOrder.Id.Value, existingOrder.OrderStatus);

        return new UpdateOrderCommandResult(true);
    }
}