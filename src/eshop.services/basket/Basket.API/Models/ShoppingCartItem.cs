namespace Basket.API.Models;

/// <summary>
/// Represents an individual item in a shopping cart, containing information such as quantity, color, product name, price, and product ID.
/// </summary>
public class ShoppingCartItem
{
    public int Quantity {get;set;}

    public string Color { get; set; } = string.Empty;

    public string ProductName {get;set;} = string.Empty;

    public decimal Price {get;set;}

    public Guid ProductId {get;set;}

    /// <summary>
    /// Optional coupon codes to apply for this item
    /// </summary>
    public List<string> CouponCodes { get; set; } = new();

    /// <summary>
    /// The original price before discounts
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// Total discount applied to this item
    /// </summary>
    public decimal TotalDiscount { get; set; }

    /// <summary>
    /// Product categories for category-based discounts
    /// </summary>
    public List<string> Categories { get; set; } = new();
}