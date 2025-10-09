
using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Represents a query to export all products to an Excel file.
/// </summary>
public record ExportProductsQuery(string? Name, decimal? MinPrice, decimal? MaxPrice, string? Category, string Format) : IQuery<ExportProductsQueryResult>;
