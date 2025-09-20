using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.DeleteBasket;

/// <summary>
/// Represents a command to delete a user's basket based on the provided username.
/// </summary>
/// <remarks>
/// This command is used within the CQRS pattern and implements the ICommand interface with a response type of DeleteBasketCommandResult.
/// </remarks>
/// <param name="UserName">The username associated with the basket to be deleted.</param>
public record DeleteBasketCommand(string UserName) : ICommand<DeleteBasketCommandResult>;