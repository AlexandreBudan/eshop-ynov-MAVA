using Marten.Schema;

namespace Basket.API.Models;

/// <summary>
/// Represents a shopping cart for a specific user, containing a collection of items and providing functionality to calculate the total price.
/// </summary>
public class ShoppingCart
{
    [Identity]
    public string UserName { get; set; } = string.Empty;
    public IEnumerable<ShoppingCartItem> Items { get; set; } = [];
    
    public decimal Total => Items.Sum(item => item.Price * item.Quantity);

    public ShoppingCart(string userName)
    {
        UserName  = userName;
    }

    public ShoppingCart()
    {
        
    }
    
}