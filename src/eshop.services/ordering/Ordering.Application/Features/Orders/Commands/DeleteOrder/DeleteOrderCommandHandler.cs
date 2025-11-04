using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Events;
using Ordering.Domain.Exceptions;
using Ordering.Domain.ValueObjects;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandHandler(IOrderingDbContext orderingDbContext) : ICommandHandler<DeleteOrderCommand, DeleteOrderCommandResult>
{
    public async Task<DeleteOrderCommandResult> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Of(request.OrderId);
        var order = await orderingDbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            throw new OrderNotFoundException(request.OrderId);
        }

        order.AddDomainEvent(new OrderCancelledEvent(order));

        orderingDbContext.Orders.Remove(order);
        await orderingDbContext.SaveChangesAsync(cancellationToken);

        return new DeleteOrderCommandResult(true);
    }
}