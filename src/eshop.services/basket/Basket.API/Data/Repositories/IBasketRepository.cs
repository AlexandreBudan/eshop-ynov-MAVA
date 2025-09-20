using Basket.API.Models;

namespace Basket.API.Data.Repositories;

/// <summary>
/// Represents a repository interface for managing shopping cart data in the system.
/// Provides methods for creating, retrieving, updating, and deleting user-specific shopping carts.
/// </summary>
public interface IBasketRepository
{
    /// <summary>
    /// Deletes the shopping cart associated with a specific user name from the data store.
    /// </summary>
    /// <param name="userName">The name of the user whose shopping cart is to be deleted.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a boolean value indicating whether the delete operation was successful.
    /// </returns>
    Task<bool> DeleteBasketAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the shopping cart for the specified user by their username.
    /// </summary>
    /// <param name="userName">The username for which the shopping cart needs to be retrieved.</param>
    /// <param name="cancellationToken">Optional. A token to cancel the asynchronous operation.</param>
    /// <returns>The shopping cart associated with the specified username, or null if no such cart exists.</returns>
    /// <exception cref="BasketNotFoundException">Thrown when no shopping cart is found for the specified username.</exception>
    Task<ShoppingCart> GetBasketByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new shopping cart for a user and persists it in the data store.
    /// </summary>
    /// <param name="basket">The shopping cart to be created, containing the user information and the items.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the created shopping cart.
    /// </returns>
    Task<ShoppingCart> CreateBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default);
}