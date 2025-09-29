using Basket.API.Data.Repositories;
using Basket.API.Models;
using BuildingBlocks.CQRS;
using Discount.Grpc;

namespace Basket.API.Features.Baskets.Commands.CreateBasket;

/// <summary>
/// Handles the creation of a shopping basket by processing the CreateBasketCommand.
/// Implements the <see cref="ICommandHandler{CreateBasketCommand, CreateBasketCommandResult}"/> interface.
/// </summary>
public class CreateBasketCommandHandler(IBasketRepository repository, DiscountProtoService.DiscountProtoServiceClient discountProtoServiceClient) : ICommandHandler<CreateBasketCommand, CreateBasketCommandResult>
{
    /// <summary>
    /// Handles the request to create a shopping basket.
    /// </summary>
    /// <param name="request">The CreateBasketCommand containing the details of the shopping basket to be created.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous operation, returning a CreateBasketCommandResult that indicates the success of the operation and includes the UserName of the created basket.</returns>
    public async Task<CreateBasketCommandResult> Handle(CreateBasketCommand request,
        CancellationToken cancellationToken)
    {
        var cart = request.Cart;

        await ApplyDiscountToItemAsync(cart, cancellationToken);

        var basketCart = await repository.CreateBasketAsync(cart, cancellationToken)
            .ConfigureAwait(false);

        return new CreateBasketCommandResult(true, basketCart.UserName);
    }

    /// <summary>
    /// Applies a discount to each item in the specified shopping cart.
    /// </summary>
    /// <param name="cart">The shopping cart containing the items to which the discount will be applied.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation of applying discounts to the items.</returns>
    private async Task ApplyDiscountToItemAsync(ShoppingCart cart, CancellationToken cancellationToken)
    {
        foreach (var item in cart.Items)
        {
            var coupon = await discountProtoServiceClient.GetDiscountAsync(new GetDiscountRequest
                { ProductName = item.ProductName }, cancellationToken: cancellationToken);
            
            item.Price -= (decimal)coupon.Amount;
        }
    }
}