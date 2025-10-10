using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.CreateBasket;

/// <summary>
/// A command to create a shopping basket, encapsulating the required details within a <see cref="ShoppingCart"/> instance.
/// </summary>
/// <remarks>
/// This command is used to initiate the creation of a new basket for a specific user, containing one or more items.
/// It implements the <see cref="ICommand{TResponse}"/> interface, where the response type is <see cref="CreateBasketCommandResult"/>.
/// </remarks>
/// <param name="Cart">
/// The shopping cart object containing user details and a collection of items to be included in the basket.
/// </param>
public record CreateBasketCommand(ShoppingCart Cart) : ICommand<CreateBasketCommandResult>;