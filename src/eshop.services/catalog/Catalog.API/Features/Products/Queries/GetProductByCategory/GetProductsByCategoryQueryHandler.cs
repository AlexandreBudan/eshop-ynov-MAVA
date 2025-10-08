using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;


namespace Catalog.API.Features.Products.Queries.GetProductByCategory;

/// <summary>
/// Handles the execution of the <see cref="GetProductByCategoryQuery"/> and retrieves the corresponding
/// product data from the data store.
/// </summary>
/// <remarks>
/// This class interacts with the database session to load the products associated with
/// the specified category in the query. If no product exists for that category,
/// a <see cref="ProductNotFoundException"/> is thrown.
/// </remarks>
public class GetProductByCategoryQueryHandler(IDocumentSession documentSession) 
    : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryQueryResult>
{
    /// <summary>
    /// Handles the execution of the GetProductByCategoryQuery and retrieves the associated products.
    /// </summary>
    /// <param name="request">The query containing the category of the products to be retrieved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing the result of the query,
    /// including the list of products.
    /// </returns>
    /// <exception cref="ProductNotFoundException">
    /// Thrown when no products are found for the given category.
    /// </exception>
    public async Task<GetProductByCategoryQueryResult> Handle(GetProductByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var products = await documentSession
            .Query<Product>()                            // IQueryable<Product>
            .Where(p => p.Categories.Contains(request.Category))
            .ToListAsync(cancellationToken);

        if (products is null || !products.Any())
        {
            throw new ProductCategoryNotFoundException(request.Category);
        }

        return new GetProductByCategoryQueryResult(products);
    }
}
