using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

/// <summary>
/// Represents a query to retrieve a paginated list of all orders.
/// </summary>
/// <param name="PaginationRequest">The pagination parameters (page index and page size).</param>
/// <returns>A paginated result containing the list of orders.</returns>
public record GetOrdersQuery(PaginationRequest PaginationRequest)
    : IQuery<PaginatedResult<OrderDto>>;
