namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
///  Represents the result of executing the <see cref="DeleteProductCommand"/>.
/// </summary>
/// <remarks>
/// This result type indicates whether the product delete operation was successful or not.
/// </remarks>
/// <param name="IsSuccessful">
/// A boolean value indicating if the delete operation completed successfully.
/// </param>
public record DeleteProductCommandResult(bool IsSuccessful);