using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

/// <summary>
/// Validator class for the <see cref="UpdateOrderCommand"/>. Validates the incoming command to ensure it meets
/// the required criteria for processing an update to an order.
/// </summary>
/// <remarks>
/// This class validates the following rules:
/// - The order identifier must not be empty.
/// - The order name must be provided.
/// - The customer identifier must not be null.
/// </remarks>
public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Order.Id).NotEmpty().WithMessage("Order Id is required");
        RuleFor(x => x.Order.OrderName).NotEmpty().WithMessage("OrderName is required");
        RuleFor(x => x.Order.CustomerId).NotNull().WithMessage("CustomerId is required");
    }
}