using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// Represents a command to delete an item from a user's shopping basket.
/// </summary>
/// <param name="UserName">The username of the user whose basket item is to be deleted.</param>
/// <param name="ProductId">The ID of the product to be removed from the basket.</param>
public record DeleteBasketItemCommand(string UserName, Guid ProductId) : ICommand<DeleteBasketItemResult>;

/// <summary>
/// Represents the result of the <see cref="DeleteBasketItemCommand"/>.
/// </summary>
/// <param name="IsSuccess">A boolean indicating whether the operation was successful.</param>
public record DeleteBasketItemResult(bool IsSuccess);
