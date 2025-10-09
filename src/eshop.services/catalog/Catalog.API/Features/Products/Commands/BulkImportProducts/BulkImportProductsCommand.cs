using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.BulkImportProducts;

/// <summary>
/// Represents the command to bulk import products from an Excel file.
/// </summary>
public class BulkImportProductsCommand : ICommand<BulkImportProductsCommandResult>
{
    /// <summary>
    /// Gets or sets the Excel file stream containing the products to import.
    /// </summary>
    public Stream FileStream { get; set; } = Stream.Null;

    /// <summary>
    /// Gets or sets the original filename of the uploaded file.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}
