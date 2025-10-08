using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Mapster;
using Marten;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Handles the DeleteProductCommand to remove an existing product from the catalog.
/// </summary>
public class DeleteProductCommandHandler(IDocumentSession documentSession) : ICommandHandler<DeleteProductCommand, DeleteProductCommandResult>
{
    /// <summary>
    /// Processes the DeleteProductCommand to remove a product from the database.
    /// </summary>
    /// <param name="request">The command containing the ID of the product to be deleted.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the delete operation.</returns>
    /// <exception cref="ProductNotFoundException">Thrown when the product to be deleted does not exist.</exception>

    public async Task<DeleteProductCommandResult> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await documentSession.LoadAsync<Product>(request.id, cancellationToken);

        if (product == null)
            throw new ProductNotFoundException(request.id);

        documentSession.Delete(product);
        await documentSession.SaveChangesAsync(cancellationToken);

        return new DeleteProductCommandResult(true);
    }
}