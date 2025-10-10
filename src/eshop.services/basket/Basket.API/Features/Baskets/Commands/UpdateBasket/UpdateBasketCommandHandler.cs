using BuildingBlocks.CQRS;
using Basket.API.Data.Repositories;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

/// <summary>
/// Handles the <see cref="UpdateBasketCommand"/> to update a user's shopping basket.
/// </summary>
public class UpdateBasketCommandHandler(IBasketRepository repository) : ICommandHandler<UpdateBasketCommand, UpdateBasketCommandResult>
{
    /// <summary>
    /// Handles the <see cref="UpdateBasketCommand"/> and updates the basket.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the command.</returns>
    public async Task<UpdateBasketCommandResult> Handle(UpdateBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasketByUserNameAsync(command.UserName, cancellationToken);
        var item = basket.Items.FirstOrDefault(i => i.ProductId == command.ProductId);

        if (item is not null)
        {
            item.Quantity = command.Quantity;
        }

        var updatedBasket = await repository.UpdateBasketAsync(basket, cancellationToken);

        return new UpdateBasketCommandResult(updatedBasket);
    }
}
