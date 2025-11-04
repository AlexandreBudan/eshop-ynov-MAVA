using Ordering.Domain.Abstractions;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Domain.Models;

/// <summary>
/// Represents a product in the domain model.
/// </summary>
public class Product : Entity<ProductId>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    public static Product Create(ProductId productId,  string name, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(price, 0, "Price must be greater than 0");
        
        return new Product
        {
            Id = productId,
            Name = name,
            Price = price,
         };
    }
}