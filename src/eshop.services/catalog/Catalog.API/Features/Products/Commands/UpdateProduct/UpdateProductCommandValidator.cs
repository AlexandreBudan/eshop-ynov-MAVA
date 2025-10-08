using FluentValidation;

namespace Catalog.API.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Validates the UpdateProductCommand to ensure that all required properties meet the defined rules and constraints.
/// </summary>
/// <remarks>
/// Utilizes FluentValidation to define validation rules for properties of the UpdateProductCommand.
/// This validator ensures that the data provided for updating a product is correct and adheres to business
/// logic constraints.
/// </remarks>

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>
    /// Provides validation rules for the UpdateProductCommand.
    /// </summary>
    /// <remarks>
    /// Ensures that the command meets necessary requirements such as non-empty properties
    /// and valid data constraints for updating a product.
    /// </remarks>
    public UpdateProductCommandValidator()
    {
        RuleFor(product => product.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(product => product.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(product => product.Categories).NotEmpty().WithMessage("Categories are required");
        RuleFor(product => product.ImageFile).NotEmpty().WithMessage("ImageFile is required");
        RuleFor(product => product.Description).NotEmpty().WithMessage("Description is required");
        RuleFor(product => product.Price).GreaterThanOrEqualTo(1).WithMessage("Price must be greater than or equal to 1");
    }
}