
using Basket.API.Extensions;
using Basket.API.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Data.Repositories;

/// <summary>
/// Represents a caching implementation of the <see cref="IBasketRepository"/> interface.
/// Provides methods for managing shopping cart data with caching capabilities to improve performance.
/// Interacts with both a distributed cache and the underlying repository.
/// </summary>
public class BasketRepositoryCache(IBasketRepository repository, IDistributedCache cache) : IBasketRepository
{
    /// <summary>
    /// The prefix used as a key identifier for managing shopping cart data in the distributed cache.
    /// This is utilized to generate unique cache keys for storing and retrieving user-specific basket data.
    /// Ensures consistency and avoids key collisions by appending user-related identifiers.
    /// </summary>
    private const string PrefixKey = "basket";

    /// <summary>
    /// Generates a unique cache key for storing and retrieving user-specific basket data.
    /// Combines a predefined prefix with the provided user name to ensure uniqueness.
    /// </summary>
    /// <param name="userName">The user name associated with the basket data.</param>
    /// <returns>A string that represents a unique cache key for the specified user.</returns>
    private static string GenerateKey(string userName) => $"{PrefixKey}_{userName}";

    /// <summary>
    /// Deletes the shopping basket associated with the specified user from the cache and underlying repository.
    /// Ensures that cached and persistent data are kept in sync by removing both.
    /// </summary>
    /// <param name="userName">The user name identifying the basket to be deleted.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A boolean indicating whether the basket was successfully deleted.</returns>
    public async Task<bool> DeleteBasketAsync(string userName, CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateKey(userName);
        await cache.RemoveAsync(cacheKey, cancellationToken);
        return await repository.DeleteBasketAsync(userName, cancellationToken);
    }

    /// <summary>
    /// Retrieves the shopping basket for the specified user.
    /// If the basket is available in the cache, it is returned directly.
    /// Otherwise, it is retrieved from the repository, cached, and then returned.
    /// </summary>
    /// <param name="userName">The user name associated with the basket to be retrieved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ShoppingCart"/> instance representing the user's shopping basket.</returns>
    public async Task<ShoppingCart> GetBasketByUserNameAsync(string userName,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateKey(userName);
        var cachedBasket = await cache.GetObjectAsync<ShoppingCart>(cacheKey, cancellationToken);

        if (cachedBasket != null)
            return cachedBasket;
            
        var basket = await repository.GetBasketByUserNameAsync(userName, cancellationToken);
        await cache.SetObjectAsync(cacheKey, basket, cancellationToken);
        return basket;
    }

    /// <summary>
    /// Creates a new shopping cart and stores it in the underlying repository and distributed cache.
    /// </summary>
    /// <param name="basket">The shopping cart to be created.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>The created shopping cart.</returns>
    public async Task<ShoppingCart> CreateBasketAsync(ShoppingCart basket,
        CancellationToken cancellationToken = default)
    {
        var createdBasket = await repository.CreateBasketAsync(basket, cancellationToken);
        var cacheKey = GenerateKey(basket.UserName);
        await cache.SetObjectAsync(cacheKey, createdBasket, cancellationToken);
        return createdBasket;
    }

    public async Task<ShoppingCart> UpdateBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        var updatedBasket = await repository.UpdateBasketAsync(basket, cancellationToken);
        var cacheKey = GenerateKey(basket.UserName);
        await cache.SetObjectAsync(cacheKey, updatedBasket, cancellationToken);
        return updatedBasket;
    }
}