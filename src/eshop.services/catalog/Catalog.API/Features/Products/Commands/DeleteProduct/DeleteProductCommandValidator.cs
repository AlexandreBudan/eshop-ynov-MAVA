using FluentValidation;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Validates the DeleteProductCommand to ensure that all required properties meet the defined rules and constraints.
/// </summary>
/// <remarks>
/// Utilizes FluentValidation to define validation rules for properties of the DeleteProductCommand.
/// This validator ensures that the data provided for deleting a product is correct and adheres to business
/// logic constraints.
/// </remarks>
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    /// <summary>
    /// Provides validation rules for the DeleteProductCommand.
    /// </summary>
    /// <remarks>
    /// Ensures that the command meets necessary requirements such as non-empty properties
    /// and valid data constraints for deleting a product.
    /// </remarks>
    public DeleteProductCommandValidator()
    {
        // Add validation rules here if needed
        RuleFor(product => product.id).NotEmpty().WithMessage("Id is required");
    }
}