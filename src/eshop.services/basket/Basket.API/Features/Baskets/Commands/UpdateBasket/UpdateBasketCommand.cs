using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

/// <summary>
/// Represents the command to update a basket.
/// </summary>
/// <param name="UserName">The user name.</param>
/// <param name="ProductId">The product id.</param>
/// <param name="Quantity">The quantity.</param>
public record UpdateBasketCommand(string UserName, Guid ProductId, int Quantity) : ICommand<UpdateBasketCommandResult>;