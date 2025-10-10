
using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Command to add an item to a user's shopping basket.
/// </summary>
/// <param name="UserName">The username of the basket owner.</param>
/// <param name="CartItem">The shopping cart item to add.</param>
/// <remarks>
/// This command is used to encapsulate the data required to add a new item to a user's basket.
/// </remarks>
public record AddItemToBasketCommand(string UserName, ShoppingCartItem CartItem) : ICommand<AddItemToBasketCommandResult>;