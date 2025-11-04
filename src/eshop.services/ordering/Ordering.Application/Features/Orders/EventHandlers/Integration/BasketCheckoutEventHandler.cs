using BuildingBlocks.Messaging.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Extensions;

namespace Ordering.Application.Features.Orders.EventHandlers.Integration;

/// <summary>
/// Handles the basket checkout integration event and maps the event data to a command for processing.
/// </summary>
/// <remarks>
/// This event handler listens for the <see cref="BasketCheckoutEvent"/> and performs the necessary actions when the event is published.
/// It maps the event data to a <see cref="CreateOrderCommand"/> and delegates the command execution using the provided <see cref="ISender"/> instance.
/// </remarks>
public class BasketCheckoutEventHandler(ISender sender, ILogger<BasketCheckoutEventHandler> logger) : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        logger.LogInformation("Integration Event Handled: {IntegrationEvent}", context.Message.GetType().Name);
        
        var commandOrder = OrderMapperExtensions.MapToCreateOrderCommand(context.Message);
        
        await sender.Send(commandOrder);
    }
}