namespace Basket.API.Models;

public class UpdateBasketItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}