namespace Basket.API.Features.Baskets.Commands.DeleteBasket;

/// <summary>
/// Represents the result of executing the DeleteBasketCommand.
/// </summary>
/// <remarks>
/// This record indicates whether the operation to delete a basket was successful.
/// It is used as the response type for the DeleteBasketCommand within the CQRS pattern.
/// </remarks>
public record DeleteBasketCommandResult(bool IsSuccess);