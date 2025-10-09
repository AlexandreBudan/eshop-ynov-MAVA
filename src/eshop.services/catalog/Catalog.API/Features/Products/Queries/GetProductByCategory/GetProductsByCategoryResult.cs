using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProductByCategory;

/// <summary>
/// Represents the result of a query to retrieve products by category.
/// Contains the retrieved <see cref="Product"/> details for the specified category.
/// </summary>
public record GetProductByCategoryQueryResult(IEnumerable<Product> Products);
