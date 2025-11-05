namespace BuildingBlocks.Messaging.Events;

public record OrderStatusUpdateNotificationEvent : IntegrationEvent
{
    public string CustomerEmail { get; set; } = null!;
    public Guid OrderId { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
}
