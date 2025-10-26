using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.CheckOutBasket;

/// <summary>
/// Validator for the <see cref="CheckOutBasketCommand"/>.
/// </summary>
/// <remarks>
/// This class defines validation rules for the <see cref="CheckOutBasketCommand"/> to ensure
/// the integrity and validity of data provided for basket checkout operations.
/// </remarks>
/// <example>
/// The validator checks:
/// - The presence of a non-null <see cref="BasketCheckoutDto"/>.
/// - That the <c>UserName</c> field in <see cref="BasketCheckoutDto"/> is not empty.
/// </example>
public class CheckOutBasketCommandValidator : AbstractValidator<CheckOutBasketCommand>
{
    public CheckOutBasketCommandValidator()
    {
        RuleFor(x => x.BasketCheckoutDto).NotNull().WithMessage("BasketCheckoutDto is required");
        RuleFor(x => x.BasketCheckoutDto.UserName).NotEmpty().WithMessage("UserName is required");
    }
}