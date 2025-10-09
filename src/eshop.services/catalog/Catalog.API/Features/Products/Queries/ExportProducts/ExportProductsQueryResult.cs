namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Represents the result of exporting products to an Excel file.
/// </summary>
/// <param name="FileContents">The byte array representing the Excel file.</param>
/// <param name="FileName">The name of the Excel file.</param>
public record ExportProductsQueryResult(byte[] FileContents, string FileName);
