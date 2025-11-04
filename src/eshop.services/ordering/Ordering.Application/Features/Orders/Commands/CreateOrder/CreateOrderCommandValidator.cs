using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Validator for the CreateOrderCommand.
/// Validates the required fields and their constraints within the CreateOrderCommand.
/// </summary>
/// <remarks>
/// This class inherits from the FluentValidation AbstractValidator, enabling detailed validation rules
/// for the CreateOrderCommand. Specifically, it enforces the following:
/// - Ensures that the OrderName in the associated Order is not empty.
/// - Ensures that the CustomerId in the associated Order is not null.
/// - Ensures that the OrderItems list in the associated Order is not empty.
/// </remarks>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Order.OrderName).NotEmpty().WithMessage("OrderName is required");
        RuleFor(x => x.Order.CustomerId).NotNull().WithMessage("CustomerId is required");
        RuleFor(x => x.Order.OrderItems).NotEmpty().WithMessage("OrderItems should not be empty");
    }
}