using Basket.API.Features.Baskets.Commands.CreateBasket;
using Basket.API.Features.Baskets.Commands.DeleteBasket;
using Basket.API.Features.Baskets.Queries.GetBasketByUserName;
using Basket.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

/// <summary>
/// The BasketsController is responsible for handling HTTP requests related to user baskets in the basket service.
/// It provides endpoints to retrieve the shopping basket for a specific user.
/// </summary>
[ApiController]
[Route("[controller]/{userName}")]
[Produces("application/json")]
public class BasketsController (ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves the shopping basket for the specified user.
    /// </summary>
    /// <param name="userName">The username whose shopping basket is to be retrieved.</param>
    /// <returns>The shopping basket associated with the specified username or a not-found response if no basket exists.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingCart>> GetBasketByUserName(string userName)
    {
        var result = await sender.Send(new GetBasketByUserNameQuery(userName));
        return Ok(result.ShoppingCart);
    }

    /// <summary>
    /// Creates a shopping basket for the specified user based on the given request data.
    /// </summary>
    /// <param name="userName">The username for whom the shopping basket is to be created.</param>
    /// <param name="request">The request containing the details of the shopping basket to be created.</param>
    /// <returns>The result of the create basket operation, including success status and associated username.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateBasketCommandResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateBasketCommandResult>> CreateBasket(string userName, [FromBody] CreateBasketCommand request)
    {
        var result = await sender.Send(request);
        return CreatedAtAction(nameof(GetBasketByUserName), new { userName }, result);
    }

    /// <summary>
    /// Deletes the shopping basket for the specified user.
    /// </summary>
    /// <param name="userName">The username whose shopping basket is to be deleted.</param>
    /// <returns>A boolean value indicating whether the basket was successfully deleted or a not-found response if no basket exists for the user.</returns>
    [HttpDelete]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteBasket(string userName)
    {
        var result = await sender.Send(new DeleteBasketCommand(userName));
        return Ok(result.IsSuccess);
    }
    
    // TODO Update basket product quantity
    
    //TODO Delete item in user basket
    
}