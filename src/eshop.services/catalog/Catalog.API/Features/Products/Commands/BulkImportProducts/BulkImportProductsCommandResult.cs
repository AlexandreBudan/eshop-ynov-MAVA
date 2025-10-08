namespace Catalog.API.Features.Products.Commands.BulkImportProducts;

/// <summary>
/// Represents the result of a bulk import operation.
/// </summary>
public class BulkImportProductsCommandResult
{
    /// <summary>
    /// Gets or sets the total number of products processed.
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of products successfully imported.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of products that failed to import.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the list of error messages encountered during import.
    /// </summary>
    public List<string> Errors { get; set; } = [];
}
