
using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Validator for the AddItemToBasketCommand, ensuring that the command's properties meet the required criteria before processing.
/// </summary>
/// <remarks>
/// This validator checks that the UserName is not empty, the CartItem is not null,
/// and that the CartItem's ProductId, Quantity, and Price are valid.
/// </remarks>
public class AddItemToBasketCommandValidator : AbstractValidator<AddItemToBasketCommand>
{
    public AddItemToBasketCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is required");
        RuleFor(x => x.CartItem).NotNull().WithMessage("CartItem is required");
        RuleFor(x => x.CartItem.ProductId).NotEmpty().WithMessage("ProductId is required");
        RuleFor(x => x.CartItem.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
        RuleFor(x => x.CartItem.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
    }
}
