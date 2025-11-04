using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Services;
using Ordering.Domain.Events;

namespace Ordering.Application.Features.Orders.EventHandlers.Domain;

public class OrderCancelledEventHandler(
    IOrderingDbContext dbContext,
    IEmailService emailService,
    ILogger<OrderCancelledEventHandler> logger) : INotificationHandler<OrderCancelledEvent>
{
    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event Handled: {DomainEvent}", notification.GetType().Name);

        try
        {
            var customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Id == notification.Order.CustomerId, cancellationToken);

            if (customer != null && !string.IsNullOrEmpty(customer.Email))
            {
                var orderDto = notification.Order.ToOrderDto();
                await emailService.SendOrderCancelledEmailAsync(orderDto, customer.Email, cancellationToken);
                logger.LogInformation("Order cancellation email sent for Order {OrderId} to {CustomerEmail}", notification.Order.Id, customer.Email);
            }
            else
            {
                logger.LogWarning("Customer email not found for Order {OrderId}", notification.Order.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send order cancellation email for Order {OrderId}", notification.Order.Id);
        }
    }
}