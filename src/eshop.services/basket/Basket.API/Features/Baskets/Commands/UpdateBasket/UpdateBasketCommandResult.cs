using Basket.API.Models;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

/// <summary>
/// Represents the result of the UpdateBasketCommand.
/// </summary>
/// <param name="Cart">The updated shopping cart.</param>
public record UpdateBasketCommandResult(ShoppingCart Cart);