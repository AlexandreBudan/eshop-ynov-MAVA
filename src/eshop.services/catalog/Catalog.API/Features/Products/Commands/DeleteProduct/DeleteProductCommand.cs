using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Represents a command to delete an existing product from the catalog.
/// </summary>
/// <remarks>
/// This command encapsulates the information required to identify and remove a product
/// from the catalog using its unique identifier.
/// </remarks>
/// <param name="id">The unique identifier of the product to be deleted.</param>
public record DeleteProductCommand(Guid id) : ICommand<DeleteProductCommandResult>;