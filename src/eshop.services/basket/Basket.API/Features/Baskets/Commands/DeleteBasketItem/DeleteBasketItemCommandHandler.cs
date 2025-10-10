using Basket.API.Data.Repositories;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// Handles the <see cref="DeleteBasketItemCommand"/> to remove an item from a user's basket.
/// </summary>
public class DeleteBasketItemCommandHandler(IBasketRepository repository)
    : ICommandHandler<DeleteBasketItemCommand, DeleteBasketItemResult>
{
    /// <summary>
    /// Handles the command to delete an item from the basket.
    /// </summary>
    /// <param name="command">The command containing the username and product ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating whether the operation was successful.</returns>
    public async Task<DeleteBasketItemResult> Handle(DeleteBasketItemCommand command, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasketByUserNameAsync(command.UserName, cancellationToken);

        var itemToRemove = basket.Items.FirstOrDefault(item => item.ProductId == command.ProductId);

        if (itemToRemove != null)
        {
            var items = basket.Items.ToList();
            items.Remove(itemToRemove);
            basket.Items = items;
            await repository.CreateBasketAsync(basket, cancellationToken);
            return new DeleteBasketItemResult(true);
        }

        return new DeleteBasketItemResult(false);
    }
}
