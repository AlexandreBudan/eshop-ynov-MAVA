using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.ValueObjects;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomer;

public class GetOrdersByCustomerQueryHandler(IOrderingDbContext orderingDbContext)
    : IQueryHandler<GetOrdersByCustomerQuery, PaginatedResult<OrderDto>>
{
    /// <summary>
    /// Handles the execution logic for retrieving orders for a specific customer.
    /// </summary>
    /// <param name="request">The query containing customer ID and pagination parameters.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A paginated result containing the list of orders for the customer.</returns>
    public async Task<PaginatedResult<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var pageIndex = request.PaginationRequest.PageIndex;
        var pageSize = request.PaginationRequest.PageSize;
        var customerId = CustomerId.Of(request.CustomerId);

        var totalCount = await orderingDbContext.Orders
            .Where(o => o.CustomerId == customerId)
            .LongCountAsync(cancellationToken);

        var orders = await orderingDbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var orderDtos = orders.Select(order => order.ToOrderDto()).ToList();

        return new PaginatedResult<OrderDto>(
            pageIndex,
            pageSize,
            totalCount,
            orderDtos);
    }
}
