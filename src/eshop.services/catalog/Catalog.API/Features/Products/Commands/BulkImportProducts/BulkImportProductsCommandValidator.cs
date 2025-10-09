using BuildingBlocks.Behaviors;
using FluentValidation;

namespace Catalog.API.Features.Products.Commands.BulkImportProducts;

/// <summary>
/// Validates the BulkImportProductsCommand to ensure the uploaded file is valid.
/// </summary>
public class BulkImportProductsCommandValidator : AbstractValidator<BulkImportProductsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BulkImportProductsCommandValidator"/> class.
    /// </summary>
    public BulkImportProductsCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File stream is required")
            .Must(stream => stream != Stream.Null && stream.Length > 0)
            .WithMessage("File stream cannot be empty");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .Must(filename => filename.EndsWith(".xlsx") || filename.EndsWith(".xls"))
            .WithMessage("Only Excel files (.xlsx, .xls) are supported");
    }
}
