using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Queries.GetProducts;

/// <summary>
/// Handles the execution of the <see cref="GetProductsQuery"/> 
/// and retrieves a paginated list of products from the data store.
/// </summary>
public class GetProductsQueryHandler(IDocumentSession documentSession) 
    : IQueryHandler<GetProductsQuery, GetProductsQueryResult>
{
    /// <summary>
    /// Executes the query to retrieve products with pagination.
    /// </summary>
    /// <param name="request">The query containing pagination parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task containing a list of products.</returns>
    public async Task<GetProductsQueryResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // Validate pagination parameters
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        var skip = (pageNumber - 1) * pageSize;

        var query = documentSession.Query<Product>();

        if (!string.IsNullOrWhiteSpace(request.Field) && !string.IsNullOrWhiteSpace(request.Value))
        {
            var field = request.Field.Trim().ToLowerInvariant();
            var value = request.Value.Trim();

            switch (field)
            {
                case "name":
                    query = (Marten.Linq.IMartenQueryable<Product>)query.Where(p => p.Name.ToLower().Contains(value.ToLower()));
                    break;
                case "price":
                    if (value.Contains(":"))
                    {
                        var range = value.Split(':');
                        if (range.Length == 2 && 
                            decimal.TryParse(range[0], out var minPrice) && 
                            decimal.TryParse(range[1], out var maxPrice))
                        {
                            query = (Marten.Linq.IMartenQueryable<Product>)query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                        }
                    }
                    else
                    {
                        if (decimal.TryParse(value, out var price))
                        {
                            query = (Marten.Linq.IMartenQueryable<Product>)query.Where(p => p.Price == price);
                        }
                    }
                    break;
            }
        }

        var products = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new GetProductsQueryResult(products);
    }
}
