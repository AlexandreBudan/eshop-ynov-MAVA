using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Queries.GetBasketByUserName;

/// <summary>
/// A query that retrieves the shopping basket associated with a specified username.
/// </summary>
/// <param name="UserName">The username for which the basket is to be retrieved.</param>
public record GetBasketByUserNameQuery(string UserName) : IQuery<GetBasketByUserNameQueryResult>;