using Catalog.API.Models;

namespace Catalog.API.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Represents the result of executing the <see cref="UpdateProductCommand"/>.
/// </summary>
/// <remarks>
/// This result type indicates whether the product update operation was successful or not.
/// </remarks>
/// <param name="Product">The updated product details if the update was successful.</param>
public record UpdateProductCommandResult(Product Product);