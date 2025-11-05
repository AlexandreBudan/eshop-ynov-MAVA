using BuildingBlocks.Messaging.Events;
using MassTransit;
using Notification.API.Services;

namespace Notification.API.Consumers;

public class OrderStatusUpdateNotificationConsumer : IConsumer<OrderStatusUpdateNotificationEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderStatusUpdateNotificationConsumer> _logger;

    public OrderStatusUpdateNotificationConsumer(
        IEmailService emailService,
        ILogger<OrderStatusUpdateNotificationConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusUpdateNotificationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received order status update notification for order {OrderId}", message.OrderId);

        try
        {
            await _emailService.SendOrderStatusUpdateEmailAsync(
                message.OrderId,
                message.OrderName,
                message.NewStatus,
                message.ShippingAddress,
                message.CustomerEmail,
                context.CancellationToken);

            _logger.LogInformation("Order status update notification processed successfully for order {OrderId}", message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order status update notification for order {OrderId}", message.OrderId);
            throw; // Let MassTransit handle retry
        }
    }
}
