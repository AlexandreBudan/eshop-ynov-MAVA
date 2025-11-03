using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler(IOrderingDbContext orderingDbContext)
    : IQueryHandler<GetOrdersQuery, PaginatedResult<OrderDto>>
{
    /// <summary>
    /// Handles the execution logic for retrieving a paginated list of orders.
    /// </summary>
    /// <param name="request">The query containing pagination parameters.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A paginated result containing the list of orders.</returns>
    public async Task<PaginatedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var pageIndex = request.PaginationRequest.PageIndex;
        var pageSize = request.PaginationRequest.PageSize;

        var totalCount = await orderingDbContext.Orders.LongCountAsync(cancellationToken);

        var orders = await orderingDbContext.Orders
            .Include(o => o.OrderItems)
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
