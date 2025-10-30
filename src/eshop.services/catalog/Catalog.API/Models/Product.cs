namespace Catalog.API.Models;

/// <summary>
/// Represents a product within the catalog. Provides details such as product name, description,
/// price, associated categories, and an image file.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the name of the image file associated with the product.
    /// </summary>
    public string ImageFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of categories associated with the product.
    /// </summary>
    public List<string> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the discount information for the product.
    /// </summary>
    public ProductDiscount? Discount { get; set; }
}

/// <summary>
/// Represents discount information for a product.
/// </summary>
public class ProductDiscount
{
    /// <summary>
    /// Gets or sets whether the product has an active discount.
    /// </summary>
    public bool HasDiscount { get; set; }

    /// <summary>
    /// Gets or sets the discount amount.
    /// </summary>
    public double Amount { get; set; }

    /// <summary>
    /// Gets or sets the discount description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the final price after applying the discount.
    /// </summary>
    public decimal FinalPrice { get; set; }
}