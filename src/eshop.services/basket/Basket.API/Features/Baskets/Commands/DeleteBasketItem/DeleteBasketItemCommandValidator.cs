using BuildingBlocks.CQRS;
using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// Validator for the <see cref="DeleteBasketItemCommand"/>.
/// </summary>
public class DeleteBasketItemCommandValidator : AbstractValidator<DeleteBasketItemCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBasketItemCommandValidator"/> class.
    /// </summary>
    public DeleteBasketItemCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is required");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required");
    }
}
