using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Events;

namespace Ordering.Application.Features.Orders.EventHandlers.Domain;

public class OrderUpdatedEventHandler(
    IOrderingDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ILogger<OrderUpdatedEventHandler> logger) : INotificationHandler<OrderUpdatedEvent>
{
    public async Task Handle(OrderUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event Handled: {DomainEvent}", notification.GetType().Name);

        try
        {
            var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == notification.Order.CustomerId, cancellationToken);

            if (customer != null && !string.IsNullOrEmpty(customer.Email))
            {
                var notificationEvent = notification.Order.ToOrderStatusUpdateNotificationEvent(customer.Email);
                await publishEndpoint.Publish(notificationEvent, cancellationToken);
                logger.LogInformation("Order status update notification event published for Order {OrderId}", notification.Order.Id);
            }
            else
            {
                logger.LogWarning("Customer email not found for Order {OrderId}", notification.Order.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish order status update notification event for Order {OrderId}", notification.Order.Id);
        }
    }
}