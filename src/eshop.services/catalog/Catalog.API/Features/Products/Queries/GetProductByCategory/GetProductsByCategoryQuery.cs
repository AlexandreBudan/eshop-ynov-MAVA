using BuildingBlocks.CQRS;
using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProductByCategory;

/// <summary>
/// Represents a query to retrieve products for a specified category.
/// This query returns a result of type <see cref="GetProductByCategoryQueryResult"/>.
/// </summary>
public record GetProductByCategoryQuery(string Category) : IQuery<GetProductByCategoryQueryResult>;