namespace BuildingBlocks.Messaging.Events;

public record OrderConfirmationNotificationEvent : IntegrationEvent
{
    public string CustomerEmail { get; set; } = null!;
    public OrderNotificationDto Order { get; set; } = null!;
}

public record OrderNotificationDto
{
    public Guid Id { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
    public PaymentInfoDto Payment { get; set; } = null!;
    public List<OrderItemNotificationDto> OrderItems { get; set; } = new();
}

public record OrderItemNotificationDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
}

public record ShippingAddressDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public record PaymentInfoDto
{
    public string CardName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
}
