using Basket.API.Data.Repositories;
using BuildingBlocks.CQRS;
using Discount.Grpc;
using Grpc.Core;

namespace Basket.API.Features.Baskets.Queries.GetBasketByUserName;

/// <summary>
/// Handles the retrieval of a shopping basket associated with a specific username.
/// Implements the <see cref="IQueryHandler{TQuery, TResponse}"/> interface to process
/// <see cref="GetBasketByUserNameQuery"/> and return a <see cref="GetBasketByUserNameQueryResult"/>.
/// </summary>
public class GetBasketByUserNameQueryHandler(IBasketRepository repository, DiscountProtoService.DiscountProtoServiceClient discountService) : IQueryHandler<GetBasketByUserNameQuery, GetBasketByUserNameQueryResult>
{
    /// <summary>
    /// Handles the execution of a query to retrieve the shopping basket associated with a specified username.
    /// </summary>
    /// <param name="request">The query request containing the username for which the basket is to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the query, which includes the shopping basket details.</returns>
    public async Task<GetBasketByUserNameQueryResult> Handle(GetBasketByUserNameQuery request,
        CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasketByUserNameAsync(request.UserName, cancellationToken)
           .ConfigureAwait(false);

        foreach (var item in basket.Items)
        {
            try
            {
                var discountRequest = new CalculateDiscountRequest
                {
                    ProductId = item.ProductId.ToString(),
                    ProductName = item.ProductName,
                    OriginalPrice = (double)item.Price, // Use item.Price as original for discount calculation
                    CouponCodes = { item.CouponCodes },
                    Categories = { item.Categories }
                };

                var discountResponse = await discountService.CalculateDiscountAsync(discountRequest, cancellationToken: cancellationToken);

                item.OriginalPrice = item.Price; // Store the price before discount as original
                item.Price = (decimal)discountResponse.FinalPrice;
                item.TotalDiscount = (decimal)discountResponse.TotalDiscount;
            }
            catch (RpcException ex)
            {
                // Log the exception (e.g., using ILogger)
                // For now, we'll just set discount to zero and keep original price
                item.OriginalPrice = item.Price;
                item.TotalDiscount = 0;
                // item.Price remains unchanged (original price)
                Console.WriteLine($"Error calling Discount service for product {item.ProductName}: {ex.Message}");
            }
        }

       return new GetBasketByUserNameQueryResult(basket);
    }
}