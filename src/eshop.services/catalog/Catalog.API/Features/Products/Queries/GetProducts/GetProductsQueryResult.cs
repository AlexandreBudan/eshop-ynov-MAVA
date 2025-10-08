using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProducts;

/// <summary>
/// Represents the result of a query that retrieves a list of products.
/// </summary>
public record GetProductsQueryResult(IEnumerable<Product> Products);
