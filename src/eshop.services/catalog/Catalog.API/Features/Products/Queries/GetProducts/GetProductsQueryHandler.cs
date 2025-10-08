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

        var products = await documentSession
            .Query<Product>()
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new GetProductsQueryResult(products);
    }
}
