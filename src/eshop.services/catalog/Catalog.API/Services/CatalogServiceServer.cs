using Catalog.Grpc;
using Discount.Grpc;
using Grpc.Core;
using Marten;

namespace Catalog.API.Services;

/// <summary>
/// The CatalogServiceServer class implements the gRPC service for retrieving catalog data with discount information.
/// It provides methods to get products with their associated discounts by communicating with the Discount Service via gRPC.
/// </summary>
public class CatalogServiceServer : CatalogProtoService.CatalogProtoServiceBase
{
    private readonly IDocumentSession _session;
    private readonly DiscountProtoService.DiscountProtoServiceClient _discountClient;
    private readonly ILogger<CatalogServiceServer> _logger;

    /// <summary>
    /// Initializes a new instance of the CatalogServiceServer class.
    /// </summary>
    /// <param name="session">The Marten document session for database operations.</param>
    /// <param name="discountClient">The gRPC client for communicating with the Discount Service.</param>
    /// <param name="logger">The logger for logging operations.</param>
    public CatalogServiceServer(
        IDocumentSession session,
        DiscountProtoService.DiscountProtoServiceClient discountClient,
        ILogger<CatalogServiceServer> logger)
    {
        _session = session;
        _discountClient = discountClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a product with its discount information.
    /// </summary>
    /// <param name="request">The request containing the product ID.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>Returns a ProductModel with discount information.</returns>
    public override async Task<ProductModel> GetProductWithDiscount(GetProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Retrieving product with discount for ProductId: {ProductId}", request.ProductId);
        var product = await _session.LoadAsync<Models.Product>(Guid.Parse(request.ProductId), context.CancellationToken) ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.ProductId} not found"));
        var productModel = new ProductModel
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description,
            Price = (double)product.Price,
            ImageFile = product.ImageFile
        };

        productModel.Categories.AddRange(product.Categories);

        // Try to calculate discount information
        try
        {
            var discountResponse = await _discountClient.CalculateDiscountAsync(new CalculateDiscountRequest
            {
                ProductId = product.Id.ToString(),
                ProductName = product.Name,
                OriginalPrice = (double)product.Price,
                Categories = { product.Categories }
            }, cancellationToken: context.CancellationToken);

            productModel.Discount = new DiscountInfo
            {
                HasDiscount = discountResponse.TotalDiscount > 0,
                Amount = discountResponse.TotalDiscount,
                Description = discountResponse.AppliedDiscounts.Count != 0
                    ? string.Join(", ", discountResponse.AppliedDiscounts.Select(d => d.Description))
                    : "No discount available",
                FinalPrice = discountResponse.FinalPrice
            };

            _logger.LogInformation("Discount calculated for product {ProductName}: Total={TotalDiscount}, Final={FinalPrice}",
                product.Name, discountResponse.TotalDiscount, discountResponse.FinalPrice);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            productModel.Discount = new DiscountInfo
            {
                HasDiscount = false,
                Amount = 0,
                Description = "No discount available",
                FinalPrice = (double)product.Price
            };

            _logger.LogInformation("No discount found for product {ProductName}", product.Name);
        }

        return productModel;
    }

    /// <summary>
    /// Retrieves products by category with discount information.
    /// </summary>
    /// <param name="request">The request containing the category name.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>Returns a ProductListModel with products and their discount information.</returns>
    public override async Task<ProductListModel> GetProductsByCategoryWithDiscount(GetProductsByCategoryRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Retrieving products by category with discounts for Category: {Category}", request.Category);

        var products = await _session.Query<Models.Product>()
            .Where(p => p.Categories.Contains(request.Category))
            .ToListAsync(context.CancellationToken);

        var productListModel = new ProductListModel();

        foreach (var product in products)
        {
            var productModel = new ProductModel
            {
                Id = product.Id.ToString(),
                Name = product.Name,
                Description = product.Description,
                Price = (double)product.Price,
                ImageFile = product.ImageFile
            };

            productModel.Categories.AddRange(product.Categories);

            // Try to calculate discount information for each product
            try
            {
                var discountResponse = await _discountClient.CalculateDiscountAsync(new CalculateDiscountRequest
                {
                    ProductId = product.Id.ToString(),
                    ProductName = product.Name,
                    OriginalPrice = (double)product.Price,
                    Categories = { product.Categories }
                }, cancellationToken: context.CancellationToken);

                productModel.Discount = new DiscountInfo
                {
                    HasDiscount = discountResponse.TotalDiscount > 0,
                    Amount = discountResponse.TotalDiscount,
                    Description = discountResponse.AppliedDiscounts.Any()
                        ? string.Join(", ", discountResponse.AppliedDiscounts.Select(d => d.Description))
                        : "No discount available",
                    FinalPrice = discountResponse.FinalPrice
                };
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                // No discount available for this product
                productModel.Discount = new DiscountInfo
                {
                    HasDiscount = false,
                    Amount = 0,
                    Description = "No discount available",
                    FinalPrice = (double)product.Price
                };
            }

            productListModel.Products.Add(productModel);
        }

        _logger.LogInformation("Retrieved {Count} products for category {Category}", productListModel.Products.Count, request.Category);

        return productListModel;
    }
}
