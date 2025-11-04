using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler(IOrderingDbContext orderingDbContext)
    : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    /// <summary>
    /// Handles the execution logic for retrieving a specific order by its ID.
    /// </summary>
    /// <param name="request">The query containing the order ID.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The order details as an OrderDto.</returns>
    /// <exception cref="NotFoundException">Thrown when the order is not found.</exception>
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Of(request.OrderId);

        var order = await orderingDbContext.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException($"Order with ID {request.OrderId} was not found.");
        }

        return order.ToOrderDto();
    }
}
