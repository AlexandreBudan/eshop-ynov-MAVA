
using Catalog.API.Models;
using Marten.Linq;

namespace Catalog.API.Features.Products.Common;

/// <summary>
/// Provides filtering methods for <see cref="Product"/> queries.
/// </summary>
public static class ProductFilter
{
    /// <summary>
    /// Applies filters to a product query.
    /// </summary>
    /// <param name="query">The query to apply filters to.</param>
    /// <param name="name">Optional: Filter by product name.</param>
    /// <param name="minPrice">Optional: Filter by minimum price.</param>
    /// <param name="maxPrice">Optional: Filter by maximum price.</param>
    /// <param name="category">Optional: Filter by category.</param>
    /// <returns>The filtered query.</returns>
    public static IMartenQueryable<Product> ApplyFilters(
        IMartenQueryable<Product> query,
        string? name,
        decimal? minPrice,
        decimal? maxPrice,
        string? category)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Categories.Contains(category));
        }

        return query;
    }
}
