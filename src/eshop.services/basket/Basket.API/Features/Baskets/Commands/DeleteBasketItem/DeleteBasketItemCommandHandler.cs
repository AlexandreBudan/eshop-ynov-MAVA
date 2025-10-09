using Basket.API.Data.Repositories;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

public class DeleteBasketItemCommandHandler(IBasketRepository repository)
    : ICommandHandler<DeleteBasketItemCommand, DeleteBasketItemResult>
{
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
        }

        return new DeleteBasketItemResult(true);
    }
}
