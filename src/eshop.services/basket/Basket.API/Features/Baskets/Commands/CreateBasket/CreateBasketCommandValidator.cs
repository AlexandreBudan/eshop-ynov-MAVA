using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.CreateBasket;

/// <summary>
/// Validator for the <see cref="CreateBasketCommand"/> class used to validate the data integrity of the command.
/// </summary>
/// <remarks>
/// Ensures that the required properties of the <see cref="CreateBasketCommand"/> are properly populated before processing.
/// Performs validation checks on the <see cref="ShoppingCart"/> instance, including:
/// - Ensuring that the cart object itself is not null.
/// - Validating that the <see cref="ShoppingCart.UserName"/> is not empty.
/// </remarks>
public class CreateBasketCommandValidator : AbstractValidator<CreateBasketCommand>
{
    public CreateBasketCommandValidator()
    {
        RuleFor(x => x.Cart).NotNull().WithMessage("Cart is required, can not be null");
        RuleFor(x => x.Cart.UserName).NotEmpty().WithMessage("UserName is required");
    }
}