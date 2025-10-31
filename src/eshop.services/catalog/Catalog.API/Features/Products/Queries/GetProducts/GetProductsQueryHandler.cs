using BuildingBlocks.CQRS;
using Catalog.API.Features.Products.Common;
using Catalog.API.Models;
using Marten;
using Discount.Grpc;
using Grpc.Core;

namespace Catalog.API.Features.Products.Queries.GetProducts;

/// <summary>
/// Handles the execution of the <see cref="GetProductsQuery"/> 
/// and retrieves a paginated list of products from the data store.
/// </summary>
public class GetProductsQueryHandler(IDocumentSession documentSession, DiscountProtoService.DiscountProtoServiceClient discountService) 
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

        query = ProductFilter.ApplyFilters(query, request.Name, request.MinPrice, request.MaxPrice, request.Category);

        var products = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            try
            {
                var discountRequest = new CalculateDiscountRequest
                {
                    ProductId = product.Id.ToString(),
                    ProductName = product.Name,
                    OriginalPrice = (double)product.Price,
                    Categories = { product.Categories }
                };

                var discountResponse = await discountService.CalculateDiscountAsync(discountRequest, cancellationToken: cancellationToken);

                product.Discount = new ProductDiscount
                {
                    HasDiscount = discountResponse.TotalDiscount > 0,
                    Amount = discountResponse.TotalDiscount,
                    Description = discountResponse.WarningMessage, // Assuming WarningMessage can be used as description
                    FinalPrice = (decimal)discountResponse.FinalPrice
                };
                product.Price = (decimal)discountResponse.FinalPrice; // Update product price to discounted price
            }
            catch (RpcException ex)
            {
                // Log the exception (e.g., using ILogger)
                // For now, we'll set discount to zero and keep original price
                product.Discount = new ProductDiscount
                {
                    HasDiscount = false,
                    Amount = 0,
                    Description = "Discount service unavailable",
                    FinalPrice = product.Price // Keep original price
                };
                Console.WriteLine($"Error calling Discount service for product {product.Name}: {ex.Message}");
            }
        }

        return new GetProductsQueryResult(products);
    }
}
