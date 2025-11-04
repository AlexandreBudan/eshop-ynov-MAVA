using Catalog.Grpc;
using Grpc.Core;
using Ordering.Application.Services;

namespace Ordering.API.Services;

/// <summary>
/// Service for communicating with Catalog.API via gRPC.
/// </summary>
public class CatalogService : ICatalogService
{
    private readonly CatalogProtoService.CatalogProtoServiceClient _catalogClient;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(
        CatalogProtoService.CatalogProtoServiceClient catalogClient,
        ILogger<CatalogService> logger)
    {
        _catalogClient = catalogClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves product details from the catalog service via gRPC.
    /// </summary>
    public async Task<ProductInfo?> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching product details for ProductId: {ProductId} from Catalog.API", productId);

            var request = new GetProductRequest { ProductId = productId };
            var response = await _catalogClient.GetProductWithDiscountAsync(request, cancellationToken: cancellationToken);

            if (response == null)
            {
                _logger.LogWarning("Product {ProductId} not found in Catalog", productId);
                return null;
            }

            var productInfo = new ProductInfo(
                Id: response.Id,
                Name: response.Name,
                Description: response.Description,
                Price: (decimal)response.Price,
                ImageFile: response.ImageFile,
                Categories: response.Categories.ToList(),
                HasDiscount: response.Discount?.HasDiscount ?? false,
                DiscountAmount: (decimal)(response.Discount?.Amount ?? 0),
                FinalPrice: (decimal)(response.Discount?.FinalPrice ?? response.Price)
            );

            _logger.LogInformation("Successfully retrieved product {ProductId}: {ProductName}", productId, productInfo.Name);
            return productInfo;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error while fetching product {ProductId}: {StatusCode} - {Message}",
                productId, ex.StatusCode, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching product {ProductId}", productId);
            return null;
        }
    }

    /// <summary>
    /// Validates that all products exist in the catalog.
    /// </summary>
    public async Task<bool> ValidateProductsAvailabilityAsync(IEnumerable<string> productIds, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating availability of {Count} products", productIds.Count());

            var validationTasks = productIds.Select(id => GetProductAsync(id, cancellationToken));
            var results = await Task.WhenAll(validationTasks);

            var allAvailable = results.All(product => product != null);

            if (!allAvailable)
            {
                var missingProducts = productIds.Where((id, index) => results[index] == null).ToList();
                _logger.LogWarning("Some products are not available: {MissingProducts}", string.Join(", ", missingProducts));
            }

            return allAvailable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while validating product availability");
            return false;
        }
    }
}
