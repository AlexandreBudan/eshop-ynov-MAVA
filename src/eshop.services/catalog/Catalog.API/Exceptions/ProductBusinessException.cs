using BuildingBlocks.Exceptions;

namespace Catalog.API.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an attempt is made to add a product
/// that already exists in the catalog. This exception is used to enforce the uniqueness
/// of product entries in the system.
/// </summary>
public class ProductAlreadyExistsException : AlreadyExistsException
{
    /// <summary>
    /// Represents an exception that is thrown when an attempt is made to add a product
    /// that already exists in the catalog. This exception is used to enforce the uniqueness
    /// of product entries in the system.
    /// </summary>
    public ProductAlreadyExistsException(string name)
        : base("Produit", "nom", name) { }
}

/// <summary>
/// Represents an exception that is thrown when a requested product cannot be found
/// in the catalog. This exception is used to indicate that an operation could not
/// be completed due to the absence of a product with the specified identifier.
/// </summary>
public class ProductNotFoundException : NotFoundException
{
    /// <summary>
    /// Represents an exception that is thrown when a requested product cannot be found
    /// in the catalog. This exception is used to indicate that an operation could not
    /// be completed due to the absence of a product with the specified identifier.
    /// </summary>
    public ProductNotFoundException(Guid id) : base("produit", id) { }
}

/// <summary>
/// Represents an exception that is thrown when no products are found 
/// for the specified category in the catalog.
/// </summary>
public class ProductCategoryNotFoundException : NotFoundException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductCategoryNotFoundException"/> class.
    /// </summary>
    /// <param name="category">The category for which no products were found.</param>
    public ProductCategoryNotFoundException(string category)
        : base("produit", $"aucun produit trouvé pour la catégorie '{category}'")
    {
    }
}
