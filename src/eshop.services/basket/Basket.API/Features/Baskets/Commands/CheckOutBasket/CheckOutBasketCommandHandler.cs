using Basket.API.Data.Repositories;
using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events;
using Mapster;
using MassTransit;

namespace Basket.API.Features.Baskets.Commands.CheckOutBasket;

/// <summary>
/// Handles the checkout process for a user's basket. This class retrieves the basket data,
/// publishes a checkout event, and removes the basket from the repository after a successful checkout.
/// </summary>
/// <remarks>
/// This command handler processes <see cref="CheckOutBasketCommand"/> requests, executes the required business logic,
/// and returns a <see cref="CheckOutBasketCommandResult"/> indicating the outcome of the operation.
/// It also integrates with the messaging system via <see cref="IPublishEndpoint"/> to notify other systems
/// about the basket checkout event.
/// </remarks>
public class CheckOutBasketCommandHandler(IBasketRepository repository, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CheckOutBasketCommand, CheckOutBasketCommandResult>
{
    /// <summary>
    /// Handles the checkout process for the user's basket, publishing a checkout event and deleting the basket upon success.
    /// </summary>
    /// <param name="request">The command request containing the basket checkout details.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the checkout process.</returns>
    public async Task<CheckOutBasketCommandResult> Handle(CheckOutBasketCommand request,
        CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasketByUserNameAsync(request.BasketCheckoutDto.UserName, cancellationToken)
            .ConfigureAwait(false);

        var eventMessage = request.BasketCheckoutDto.Adapt<BasketCheckoutEvent>();
        eventMessage.TotalPrice = basket.Total;

        // Map basket items to the event
        eventMessage.Items = basket.Items.Select(item => new BasketItem
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            ProductName = item.ProductName,
            Price = item.Price,
            Color = item.Color
        }).ToList();

        await publishEndpoint.Publish(eventMessage, cancellationToken).ConfigureAwait(false);

        await repository.DeleteBasketAsync(request.BasketCheckoutDto.UserName, cancellationToken).ConfigureAwait(false);

        return new CheckOutBasketCommandResult(true);
    }
}