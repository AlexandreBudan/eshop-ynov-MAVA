using BuildingBlocks.CQRS;
using Basket.API.Data.Repositories;

namespace Basket.API.Features.Baskets.Commands.UpdateBasket;

public class UpdateBasketCommandHandler(IBasketRepository repository) : ICommandHandler<UpdateBasketCommand, UpdateBasketCommandResult>
{
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
