namespace Basket.API.Features.Baskets.Commands.CreateBasket;

/// <summary>
/// Represents the result of executing a command to create a basket.
/// </summary>
public record CreateBasketCommandResult(bool IsSuccess, string UserName);