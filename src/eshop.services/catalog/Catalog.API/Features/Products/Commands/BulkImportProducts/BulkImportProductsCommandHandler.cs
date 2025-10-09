using System.Globalization;
using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;
using OfficeOpenXml;

namespace Catalog.API.Features.Products.Commands.BulkImportProducts;

/// <summary>
/// Handles the BulkImportProducts command to import multiple products from an Excel file.
/// </summary>
public class BulkImportProductsCommandHandler(IDocumentSession documentSession, ILogger<BulkImportProductsCommandHandler> logger)
    : ICommandHandler<BulkImportProductsCommand, BulkImportProductsCommandResult>
{
    /// <summary>
    /// Processes the bulk import of products from an Excel file.
    /// </summary>
    /// <param name="request">The command containing the Excel file stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing import statistics and any errors.</returns>
    public async Task<BulkImportProductsCommandResult> Handle(BulkImportProductsCommand request, CancellationToken cancellationToken)
    {
        var result = new BulkImportProductsCommandResult();

        // Configure EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        try
        {
            using var package = new ExcelPackage(request.FileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                result.Errors.Add("No worksheet found in the Excel file");
                return result;
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount <= 1)
            {
                result.Errors.Add("No data rows found in the Excel file");
                return result;
            }

            var existingProducts = await documentSession.Query<Product>()
                .Select(p => p.Name)
                .ToListAsync(cancellationToken);
            var existingNamesSet = new HashSet<string>(existingProducts, StringComparer.OrdinalIgnoreCase);
            for (int row = 2; row <= rowCount; row++)
            {
                result.TotalProcessed++;

                try
                {
                    var name = worksheet.Cells[row, 1].Text?.Trim();
                    var description = worksheet.Cells[row, 2].Text?.Trim();
                    var priceText = worksheet.Cells[row, 3].Text?.Trim();
                    var imageFile = worksheet.Cells[row, 4].Text?.Trim();
                    var categoriesText = worksheet.Cells[row, 5].Text?.Trim();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        result.Errors.Add($"Row {row}: Name is required");
                        result.FailureCount++;
                        continue;
                    }

                    var normalizedPrice = priceText?.Replace(',', '.');
                    if (!decimal.TryParse(normalizedPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) || price <= 0)
                    {
                        result.Errors.Add($"Row {row}: Invalid price value '{priceText}'");
                        result.FailureCount++;
                        continue;
                    }

                    if (existingNamesSet.Contains(name))
                    {
                        result.Errors.Add($"Row {row}: Product '{name}' already exists");
                        result.FailureCount++;
                        continue;
                    }

                    var categories = string.IsNullOrWhiteSpace(categoriesText)
                        ? []
                        : categoriesText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .Where(c => !string.IsNullOrWhiteSpace(c))
                            .ToList();

                    var product = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = description ?? string.Empty,
                        Price = price,
                        ImageFile = imageFile ?? string.Empty,
                        Categories = categories
                    };

                    documentSession.Store(product);
                    existingNamesSet.Add(name);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing row {Row}", row);
                    result.Errors.Add($"Row {row}: {ex.Message}");
                    result.FailureCount++;
                }
            }

            if (result.SuccessCount > 0)
            {
                await documentSession.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading Excel file");
            result.Errors.Add($"Error reading Excel file: {ex.Message}");
        }

        return result;
    }
}
