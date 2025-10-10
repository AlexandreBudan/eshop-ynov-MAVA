using Basket.API.Models;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Represents the result of executing a command to add an item to a basket.
/// </summary>
/// <param name="ShoppingCart">The updated shopping cart.</param>
public record AddItemToBasketCommandResult(ShoppingCart ShoppingCart);