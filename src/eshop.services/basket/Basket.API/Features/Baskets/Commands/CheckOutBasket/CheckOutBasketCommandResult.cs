using Basket.API.Models;

namespace Basket.API.Features.Baskets.Commands.CheckOutBasket;

/// <summary>
/// Represents the result of a basket checkout command.
/// </summary>
/// <remarks>
/// This result includes details about the checked-out basket including total price and items.
/// </remarks>
/// <param name="IsSuccess">
/// A boolean value that specifies the success status of the checkout operation.
/// </param>
/// <param name="UserName">
/// The username associated with the checked-out basket.
/// </param>
/// <param name="TotalPrice">
/// The total price of all items in the basket at checkout.
/// </param>
/// <param name="Items">
/// The list of items that were in the basket at checkout.
/// </param>
public record CheckOutBasketCommandResult(
    bool IsSuccess,
    string UserName,
    decimal TotalPrice,
    IEnumerable<ShoppingCartItem> Items);