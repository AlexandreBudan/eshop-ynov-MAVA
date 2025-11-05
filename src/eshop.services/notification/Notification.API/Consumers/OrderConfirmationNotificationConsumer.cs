using BuildingBlocks.Messaging.Events;
using MassTransit;
using Notification.API.Services;

namespace Notification.API.Consumers;

public class OrderConfirmationNotificationConsumer : IConsumer<OrderConfirmationNotificationEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderConfirmationNotificationConsumer> _logger;

    public OrderConfirmationNotificationConsumer(
        IEmailService emailService,
        ILogger<OrderConfirmationNotificationConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderConfirmationNotificationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received order confirmation notification for order {OrderId}", message.Order.Id);

        try
        {
            await _emailService.SendOrderConfirmationEmailAsync(
                message.Order,
                message.CustomerEmail,
                context.CancellationToken);

            _logger.LogInformation("Order confirmation notification processed successfully for order {OrderId}", message.Order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order confirmation notification for order {OrderId}", message.Order.Id);
            throw; // Let MassTransit handle retry
        }
    }
}
