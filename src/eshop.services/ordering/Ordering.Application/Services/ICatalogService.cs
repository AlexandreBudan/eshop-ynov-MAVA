namespace Ordering.Application.Services;

/// <summary>
/// Interface for catalog service operations via gRPC.
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Retrieves product details from the catalog service.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Product details including name, price, and discount information.</returns>
    Task<ProductInfo?> GetProductAsync(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that all products are available in the catalog.
    /// </summary>
    /// <param name="productIds">List of product IDs to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if all products exist and are available.</returns>
    Task<bool> ValidateProductsAvailabilityAsync(IEnumerable<string> productIds, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents product information retrieved from the catalog.
/// </summary>
public record ProductInfo(
    string Id,
    string Name,
    string Description,
    decimal Price,
    string ImageFile,
    List<string> Categories,
    bool HasDiscount,
    decimal DiscountAmount,
    decimal FinalPrice);
