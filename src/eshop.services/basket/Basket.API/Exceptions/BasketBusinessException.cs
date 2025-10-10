using BuildingBlocks.Exceptions;

namespace Basket.API.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a basket associated with a specific user
/// cannot be found in the system. This exception is typically used to indicate that the
/// requested basket resource does not exist or is inaccessible.
/// </summary>
public class BasketNotFoundException : NotFoundException
{
    /// <summary>
    /// Represents an exception that is thrown when a basket associated with a specific user
    /// cannot be found in the system. This exception is typically used to indicate that the
    /// requested basket resource does not exist or is inaccessible.
    /// </summary>
    public BasketNotFoundException(string userName) : base("panier", userName) { }
}