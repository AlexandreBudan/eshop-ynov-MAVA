using Basket.API.Features.Baskets.Dtos;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.CheckOutBasket;

/// <summary>
/// Represents a command to process the checkout operation for a user's basket.
/// </summary>
/// <remarks>
/// This command encapsulates the required data for checking out a basket, including the user's details,
/// basket information, and payment details.
/// </remarks>
/// <param name="BasketCheckoutDto">
/// Contains the details of the user's basket and payment information required for checkout.
/// </param>
public record CheckOutBasketCommand(BasketCheckoutDto BasketCheckoutDto) : ICommand<CheckOutBasketCommandResult>;