using BuildingBlocks.CQRS;
using Catalog.API.Features.Products.Common;
using Catalog.API.Models;
using Marten;
using OfficeOpenXml;
using System.Text;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Handles the <see cref="ExportProductsQuery"/> to generate an Excel file of all products.
/// </summary>
public class ExportProductsQueryHandler(IDocumentSession documentSession)
    : IQueryHandler<ExportProductsQuery, ExportProductsQueryResult>
{
    /// <summary>
    /// Handles the query to export all products to an Excel file.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="ExportProductsQueryResult"/> containing the Excel file contents and name.</returns>
    public async Task<ExportProductsQueryResult> Handle(ExportProductsQuery request, CancellationToken cancellationToken)
    {

        var query = documentSession.Query<Product>();

        query = (Marten.Linq.IMartenQueryable<Product>)ProductFilter.ApplyFilters(query, request.Name, request.MinPrice, request.MaxPrice, request.Category);

        var products = await query.ToListAsync(cancellationToken);

        byte[] fileContents;
        string fileName;

        if (request.Format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new StringBuilder();
            builder.AppendLine("Name,Description,Price,Categories");

            foreach (var product in products)
            {
                builder.AppendLine($"{product.Name},{product.Description},{product.Price},{string.Join(",", product.Categories)}");
            }

            fileContents = Encoding.UTF8.GetBytes(builder.ToString());
            fileName = $"products_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        }
        else
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            
            var worksheet = package.Workbook.Worksheets.Add("Products");
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Description";
            worksheet.Cells[1, 3].Value = "Price";
            worksheet.Cells[1, 4].Value = "Categories";
            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];
                worksheet.Cells[i + 2, 1].Value = product.Name;
                worksheet.Cells[i + 2, 2].Value = product.Description;
                worksheet.Cells[i + 2, 3].Value = product.Price;
                worksheet.Cells[i + 2, 4].Value = string.Join(", ", product.Categories);
            }

            fileContents = await package.GetAsByteArrayAsync(cancellationToken);
            fileName = $"products_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        }
        return new ExportProductsQueryResult(fileContents, fileName);
    }
}
