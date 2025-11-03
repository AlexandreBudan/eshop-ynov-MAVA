using BuildingBlocks.CQRS;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Represents a query to retrieve a specific order by its unique identifier.
/// </summary>
/// <param name="OrderId">The unique identifier of the order.</param>
/// <returns>The order details as an OrderDto.</returns>
public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto>;
