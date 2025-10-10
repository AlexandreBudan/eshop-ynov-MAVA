using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

/// <summary>
/// Validator for the <see cref="UpdateBasketCommand"/>.
/// </summary>
public class UpdateBasketCommandValidator : AbstractValidator<UpdateBasketCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBasketCommandValidator"/> class.
    /// </summary>
    public UpdateBasketCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is required");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
    }
}
