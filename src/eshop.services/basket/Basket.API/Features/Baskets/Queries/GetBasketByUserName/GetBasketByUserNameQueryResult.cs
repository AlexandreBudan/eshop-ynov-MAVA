using Basket.API.Models;

namespace Basket.API.Features.Baskets.Queries.GetBasketByUserName;

/// <summary>
/// Represents the result of a query to retrieve the basket for a specified user.
/// </summary>
public record GetBasketByUserNameQueryResult(ShoppingCart ShoppingCart);