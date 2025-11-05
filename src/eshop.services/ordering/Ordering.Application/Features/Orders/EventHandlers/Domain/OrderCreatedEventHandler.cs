using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Domain.Events;

namespace Ordering.Application.Features.Orders.EventHandlers.Domain;

/// <summary>
/// Handles the domain event for an order being created.
/// This handler is responsible for processing the <see cref="OrderCreatedEvent"/>
/// and publishing an integration event based on the order details.
/// </summary>
public class OrderCreatedEventHandler(
    IPublishEndpoint publishEndpoint,
    IFeatureManager featureManager,
    ILogger<OrderCreatedEventHandler> logger,
    IOrderingDbContext dbContext) : INotificationHandler<OrderCreatedEvent>
{
    /// <summary>
    /// Handles the domain event when a new order is created.
    /// </summary>
    /// <param name="notification">The <see cref="OrderCreatedEvent"/> containing details of the created order.</param>
    /// <param name="cancellationToken">A cancellation token to observe while performing the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event Handled: {DomainEvent}", notification.GetType().Name);

        if (await featureManager.IsEnabledAsync("OrderFulfilment"))
        {
            var orderCreatedIntegrationEvent = notification.Order.ToOrderDto();
            await publishEndpoint.Publish(orderCreatedIntegrationEvent, cancellationToken);
        }

        // Publish order confirmation notification event to RabbitMQ
        try
        {
            var customer = await dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == notification.Order.CustomerId, cancellationToken);

            if (customer != null && !string.IsNullOrEmpty(customer.Email))
            {
                var notificationEvent = notification.Order.ToOrderConfirmationNotificationEvent(customer.Email);
                await publishEndpoint.Publish(notificationEvent, cancellationToken);
                logger.LogInformation("Order confirmation notification event published for Order {OrderId}",
                    notification.Order.Id);
            }
            else
            {
                logger.LogWarning("Customer email not found for Order {OrderId}", notification.Order.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish order confirmation notification event for Order {OrderId}", notification.Order.Id);
        }
    }
}