using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Features.Orders.Commands.CreateOrder;
using Ordering.Application.Features.Orders.Commands.DeleteOrder;
using Ordering.Application.Features.Orders.Commands.UpdateOrder;
using Ordering.Application.Features.Orders.Dtos;

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
    /// Retrieves a list of orders filtered by the provided order name.
    /// </summary>
    /// <param name="name">The name used to filter the orders.</param>
    /// <returns>A collection of <see cref="OrderDto"/> objects that match the specified name.</returns>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByName(string name)
    {
        // TODO
        return Ok();
    }

    /// <summary>
    /// Retrieves a list of orders associated with a specific customer identified by their unique ID.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer whose orders are being retrieved.</param>
    /// <returns>A collection of <see cref="OrderDto"/> objects associated with the specified customer.</returns>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCustomerId(Guid customerId)
    {
        // TODO
        return Ok();
    }


    /// <summary>
    /// Retrieves a paginated list of orders based on the specified page index and page size.
    /// </summary>
    /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
    /// <param name="pageSize">The number of orders to include in each page of results.</param>
    /// <returns>A collection of <see cref="OrderDto"/> objects representing the paginated list of orders.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] int pageIndex ,[FromQuery]  int pageSize)
    {
        // TODO
        return Ok();
    }

    /// <summary>
    /// Creates a new order based on the provided order details.
    /// </summary>
    /// <param name="order">The <see cref="OrderDto"/> containing details of the order to be created.</param>
    /// <returns>The unique identifier of the newly created order.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateOrder([FromBody] OrderDto order)
    {
        var result = await sender.Send(new CreateOrderCommand(order));
        return Ok(result.NewOrderId);
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
}