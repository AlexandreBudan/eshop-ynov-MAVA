using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Mapster;
using Marten;

namespace Catalog.API.Features.Products.Commands.CreateProduct;

/// <summary>
/// Handles the CreateProduct command to create a new product in the system by persisting it through the provided document session.
/// </summary>
public class CreateProductCommandHandler(IDocumentSession documentSession): ICommandHandler<CreateProductCommand, CreateProductCommandResult>
{
    /// <summary>
    /// Handles the processing of the CreateProduct command, which adds a new product to the system.
    /// It ensures the product does not already exist and persists it to the database.
    /// </summary>
    /// <param name="request">The CreateProduct command containing the details of the product to create.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the operation, containing the result of the command which includes the product ID.</returns>
    /// <exception cref="ProductAlreadyExistsException">Thrown when a product with the same name already exists in the system.</exception>
    public async Task<CreateProductCommandResult> Handle(CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = request.Adapt<Product>();

        var check = await documentSession.Query<Product>()
            .AnyAsync(x => x.Name.Equals(product.Name, StringComparison.CurrentCultureIgnoreCase),cancellationToken);

        if(check)
            throw new ProductAlreadyExistsException(product.Name);
        
        documentSession.Store(product);
        
        await documentSession.SaveChangesAsync(cancellationToken);

        return new CreateProductCommandResult(product);
    }
}