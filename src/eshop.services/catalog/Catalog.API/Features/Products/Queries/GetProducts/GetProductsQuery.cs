using BuildingBlocks.CQRS;
using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProducts;

/// <summary>
/// Represents a query to retrieve a paginated list of products.
/// </summary>
public record GetProductsQuery(int PageNumber, int PageSize, string? Name, decimal? MinPrice, decimal? MaxPrice, string? Category) 
    : IQuery<GetProductsQueryResult>;
