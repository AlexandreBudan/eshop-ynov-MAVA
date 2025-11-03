using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Services;

/// <summary>
/// Interface for email service operations.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an order confirmation email to the customer.
    /// </summary>
    /// <param name="order">The order details to include in the email.</param>
    /// <param name="customerEmail">The email address of the customer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendOrderConfirmationEmailAsync(OrderDto order, string customerEmail, CancellationToken cancellationToken = default);
}
