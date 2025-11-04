using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Services;

/// <summary>
/// Service for managing email retry queue.
/// </summary>
public interface IEmailRetryService
{
    /// <summary>
    /// Adds a failed email to the retry queue.
    /// </summary>
    /// <param name="orderId">The ID of the order related to the email.</param>
    /// <param name="recipientEmail">The email address of the recipient.</param>
    /// <param name="order">The order DTO containing order details for the email.</param>
    /// <param name="error">The error message from the failed attempt.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task QueueFailedEmailAsync(
        Guid orderId,
        string recipientEmail,
        OrderDto order,
        string error,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets emails that are ready to be retried.
    /// </summary>
    /// <param name="batchSize">Maximum number of emails to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of failed emails ready for retry.</returns>
    Task<List<(Guid FailedEmailId, Guid OrderId, string RecipientEmail, string EmailSubject, string EmailBody)>> GetEmailsReadyForRetryAsync(
        int batchSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an email as successfully sent.
    /// </summary>
    Task MarkEmailAsSentAsync(Guid failedEmailId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an email as failed with an error message.
    /// </summary>
    Task MarkEmailAsFailedAsync(Guid failedEmailId, string error, CancellationToken cancellationToken = default);
}
