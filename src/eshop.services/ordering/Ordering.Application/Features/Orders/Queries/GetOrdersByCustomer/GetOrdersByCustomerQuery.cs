using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomer;

/// <summary>
/// Represents a query to retrieve a paginated list of orders for a specific customer.
/// </summary>
/// <param name="CustomerId">The unique identifier of the customer.</param>
/// <param name="PaginationRequest">The pagination parameters (page index and page size).</param>
/// <returns>A paginated result containing the list of orders for the customer.</returns>
public record GetOrdersByCustomerQuery(Guid CustomerId, PaginationRequest PaginationRequest)
    : IQuery<PaginatedResult<OrderDto>>;
