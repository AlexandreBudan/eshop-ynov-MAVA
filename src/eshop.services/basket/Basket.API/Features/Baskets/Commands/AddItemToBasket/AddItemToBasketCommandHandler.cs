using Basket.API.Data.Repositories;
using Basket.API.Models;
using BuildingBlocks.CQRS;
using Discount.Grpc;

namespace Basket.API.Features.Baskets.Commands.AddItemToBasket;

/// <summary>
/// Handles the command to add an item to a user's basket.
/// </summary>
/// <remarks>
/// If the item already exists in the basket, its quantity is increased. Otherwise, the item is added to the basket.
/// The updated basket is then persisted using the repository.
/// </remarks>
/// <param name="repository">The repository used to access and update basket data.</param>
/// <param name="discountProtoServiceClient">The gRPC client for discount service.</param>
public class AddItemToBasketCommandHandler(
    IBasketRepository repository,
    DiscountProtoService.DiscountProtoServiceClient discountProtoServiceClient) : ICommandHandler<AddItemToBasketCommand, AddItemToBasketCommandResult>
{
    /// <summary>
    /// Handles the `AddItemToBasketCommand`. It retrieves the user's basket, adds the specified item
    /// or updates its quantity if it already exists, and persists the changes.
    /// </summary>
    /// <param name="command">The command containing the username and the shopping cart item to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated shopping cart.</returns>
    public async Task<AddItemToBasketCommandResult> Handle(AddItemToBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasketByUserNameAsync(command.UserName, cancellationToken);

        var item = basket.Items.FirstOrDefault(x => x.ProductId == command.CartItem.ProductId);

        if (item != null)
        {
            item.Quantity += command.CartItem.Quantity;
        } else
        {
            await ApplyDiscountToItemAsync(command.CartItem, cancellationToken);
            var items = basket.Items.ToList();
            items.Add(command.CartItem);
            basket.Items = items;
        }

        await repository.CreateBasketAsync(basket, cancellationToken);

        return new AddItemToBasketCommandResult(basket);
    }

    /// <summary>
    /// Applies a discount to a shopping cart item by calling the discount service via gRPC.
    /// </summary>
    /// <param name="item">The shopping cart item to apply the discount to.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation of applying discount to the item.</returns>
    private async Task ApplyDiscountToItemAsync(ShoppingCartItem item, CancellationToken cancellationToken)
    {
        try
        {
            if (item.OriginalPrice == 0)
            {
                item.OriginalPrice = item.Price;
            }

            var calculateDiscountRequest = new CalculateDiscountRequest
            {
                ProductName = item.ProductName,
                ProductId = item.ProductId.ToString(),
                OriginalPrice = (double)item.OriginalPrice,
            };
            calculateDiscountRequest.CouponCodes.AddRange(item.CouponCodes);
            calculateDiscountRequest.Categories.AddRange(item.Categories);

            var discountResponse = await discountProtoServiceClient.CalculateDiscountAsync(calculateDiscountRequest, cancellationToken: cancellationToken);

            item.TotalDiscount = (decimal)discountResponse.TotalDiscount;
            item.Price = (decimal)discountResponse.FinalPrice;

            if (discountResponse.HasWarning)
            {
                Console.WriteLine($"Discount warning for {item.ProductName}: {discountResponse.WarningMessage}");
            }
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            item.TotalDiscount = 0;
        }
    }
}