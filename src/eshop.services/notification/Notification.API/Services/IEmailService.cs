using BuildingBlocks.Messaging.Events;

namespace Notification.API.Services;

public interface IEmailService
{
    Task SendOrderConfirmationEmailAsync(OrderNotificationDto order, string customerEmail, CancellationToken cancellationToken = default);
    Task SendOrderStatusUpdateEmailAsync(Guid orderId, string orderName, string newStatus, ShippingAddressDto shippingAddress, string customerEmail, CancellationToken cancellationToken = default);
    Task SendOrderCancelledEmailAsync(Guid orderId, string orderName, ShippingAddressDto shippingAddress, string customerEmail, CancellationToken cancellationToken = default);
}
