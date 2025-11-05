using BuildingBlocks.Messaging.Events;
using MassTransit;
using Notification.API.Services;

namespace Notification.API.Consumers;

public class OrderCancelledNotificationConsumer : IConsumer<OrderCancelledNotificationEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCancelledNotificationConsumer> _logger;

    public OrderCancelledNotificationConsumer(
        IEmailService emailService,
        ILogger<OrderCancelledNotificationConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledNotificationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received order cancelled notification for order {OrderId}", message.OrderId);

        try
        {
            await _emailService.SendOrderCancelledEmailAsync(
                message.OrderId,
                message.OrderName,
                message.ShippingAddress,
                message.CustomerEmail,
                context.CancellationToken);

            _logger.LogInformation("Order cancelled notification processed successfully for order {OrderId}", message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order cancelled notification for order {OrderId}", message.OrderId);
            throw; // Let MassTransit handle retry
        }
    }
}
