using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.DeleteOrder;

/// <summary>
/// Validator for the <see cref="DeleteOrderCommand"/> command.
/// </summary>
/// <remarks>
/// This class ensures that the <see cref="DeleteOrderCommand"/> contains
/// all required and valid data for processing the delete operation.
/// </remarks>
public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order Id is required");
    }
}