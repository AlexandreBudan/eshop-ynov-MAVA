using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Mapster;
using Marten;

namespace Catalog.API.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Handles the UpdateProductCommand to update an existing product in the catalog.
/// </summary>
public class UpdateProductCommandHandler(IDocumentSession documentSession) : ICommandHandler<UpdateProductCommand, UpdateProductCommandResult>
{
    /// <summary>
    /// Processes the UpdateProductCommand to update a product's details in the database.
    /// </summary>
    /// <param name="request">The command containing the updated product information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the update operation.</returns>
    /// <exception cref="ProductNotFoundException">Thrown when the product to be updated does not exist.</exception>

    public async Task<UpdateProductCommandResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // check if product exists
        var product = await documentSession.LoadAsync<Product>(request.Id, cancellationToken);

        if (product == null)
        {
            throw new ProductNotFoundException(request.Id);
        }

        product = request.Adapt<Product>();
        documentSession.Store(product);

        await documentSession.SaveChangesAsync(cancellationToken);

        return new UpdateProductCommandResult(true);
    }
}