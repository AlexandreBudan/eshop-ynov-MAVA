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
}