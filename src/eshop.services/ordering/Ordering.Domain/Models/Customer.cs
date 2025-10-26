using Ordering.Domain.Abstractions;
using Ordering.Domain.ValueObjects;

namespace Ordering.Domain.Models;

/// <summary>
/// Represents a customer entity within the domain.
/// </summary>
/// <remarks>
/// The Customer entity is used to model domain-specific information about a user or purchaser within the Ordering context.
/// This class encapsulates core properties such as the customer's name and email and provides a factory method for creation.
/// </remarks>
public class Customer : Entity<CustomerId>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public static Customer Create(CustomerId customerId,  string name, string email)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(email);
        
        return new Customer
        {
            Id = customerId,
            Name = name,
            Email = email,
        };
        
    }
}