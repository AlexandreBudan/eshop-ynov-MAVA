using BuildingBlocks.Pagination;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Features.Orders.Commands.CreateOrder;
using Ordering.Application.Features.Orders.Commands.DeleteOrder;
using Ordering.Application.Features.Orders.Commands.UpdateOrder;
using Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Application.Features.Orders.Queries.GetOrderById;
using Ordering.Application.Features.Orders.Queries.GetOrders;
using Ordering.Application.Features.Orders.Queries.GetOrdersByCustomer;
using Ordering.Domain.Enums;

namespace Ordering.API.Controllers;

/// <summary>
/// API controller responsible for managing and accessing order data.
/// Provides endpoints for retrieving, creating, and deleting orders associated with customers or specific order names.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class OrdersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves a specific order by its unique identifier.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to retrieve.</param>
    /// <returns>An <see cref="OrderDto"/> object representing the order details.</returns>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid orderId)
    {
        var result = await sender.Send(new GetOrderByIdQuery(orderId));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a paginated list of orders associated with a specific customer identified by their unique ID.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose orders are being retrieved.</param>
    /// <param name="pageIndex">The page index for pagination (default: 1).</param>
    /// <param name="pageSize">The page size for pagination (default: 10).</param>
    /// <returns>A paginated collection of <see cref="OrderDto"/> objects associated with the specified customer.</returns>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(PaginatedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<OrderDto>>> GetOrdersByCustomerId(
        Guid customerId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await sender.Send(new GetOrdersByCustomerQuery(
            customerId,
            new PaginationRequest(pageIndex, pageSize)));
        return Ok(result);
    }


    /// <summary>
    /// Retrieves a paginated list of all orders based on the specified page index and page size.
    /// </summary>
    /// <param name="pageIndex">The page index for pagination (default: 1).</param>
    /// <param name="pageSize">The number of orders to include in each page of results (default: 10).</param>
    /// <returns>A paginated collection of <see cref="OrderDto"/> objects representing the list of orders.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<OrderDto>>> GetOrders(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await sender.Send(new GetOrdersQuery(
            new PaginationRequest(pageIndex, pageSize)));
        return Ok(result);
    }

    /// <summary>
    /// Creates a new order based on the provided order details.
    /// </summary>
    /// <param name="order">The <see cref="OrderDto"/> containing details of the order to be created.</param>
    /// <returns>The created order with HTTP 201 status.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderDto order)
    {
        var result = await sender.Send(new CreateOrderCommand(order));

        // Retrieve the created order to return the full object
        var createdOrder = await sender.Send(new GetOrderByIdQuery(result.NewOrderId));

        return CreatedAtAction(nameof(GetOrderById), new { orderId = result.NewOrderId }, createdOrder);
    }

    /// <summary>
    /// Updates an existing order with the provided order details.
    /// </summary>
    /// <param name="order">The updated order details encapsulated in an <see cref="OrderDto"/>.</param>
    /// <returns>A boolean result indicating whether the update operation was successful.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> UpdateOrder([FromBody] OrderDto order)
    {
        var result = await sender.Send(new UpdateOrderCommand(order));
        return Ok(result.IsSuccess);
    }

    /// <summary>
    /// Deletes an order based on the provided order identifier.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    [HttpDelete("{orderId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteOrder(Guid orderId)
    {
        var result = await sender.Send(new DeleteOrderCommand(orderId));
        return Ok(result.IsSuccess);
    }

    /// <summary>
    /// Updates the status of an existing order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order whose status is to be updated.</param>
    /// <param name="newStatus">The new status to set for the order.</param>
    /// <returns>A boolean indicating whether the status update was successful.</returns>
    [HttpPatch("{orderId:guid}/status")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> UpdateOrderStatus(Guid orderId, [FromQuery] OrderStatus newStatus)
    {
        var result = await sender.Send(new UpdateOrderStatusCommand(orderId, newStatus));
        return Ok(result.IsSuccess);
    }
}