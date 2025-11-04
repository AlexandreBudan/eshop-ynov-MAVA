using Catalog.API.Models;

namespace Catalog.API.Features.Products.Commands.CreateProduct;

/// <summary>
/// Represents the result of the CreateProduct command execution.
/// </summary>
public record CreateProductCommandResult(Product Product);