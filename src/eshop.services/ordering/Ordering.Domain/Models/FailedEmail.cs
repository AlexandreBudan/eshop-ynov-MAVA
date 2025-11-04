using Ordering.Domain.Abstractions;

namespace Ordering.Domain.Models;

/// <summary>
/// Represents an email that failed to send and needs to be retried.
/// </summary>
public class FailedEmail : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public string RecipientEmail { get; private set; } = string.Empty;
    public string EmailSubject { get; private set; } = string.Empty;
    public string EmailBody { get; private set; } = string.Empty;
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; } = 3;
    public DateTime? LastRetryAt { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public string? LastError { get; private set; }
    public EmailStatus Status { get; private set; }

    // Constructor for EF Core
    private FailedEmail() { }

    /// <summary>
    /// Creates a new failed email entry.
    /// </summary>
    public static FailedEmail Create(
        Guid orderId,
        string recipientEmail,
        string emailSubject,
        string emailBody,
        string error,
        int maxRetries = 3)
    {
        var failedEmail = new FailedEmail
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            RecipientEmail = recipientEmail,
            EmailSubject = emailSubject,
            EmailBody = emailBody,
            RetryCount = 0,
            MaxRetries = maxRetries,
            CreatedAt = DateTime.UtcNow, // From base Entity class
            LastError = error,
            Status = EmailStatus.Pending,
            NextRetryAt = DateTime.UtcNow.AddMinutes(5) // First retry after 5 minutes
        };

        return failedEmail;
    }

    /// <summary>
    /// Marks the email as being retried and calculates the next retry time using exponential backoff.
    /// </summary>
    public void MarkAsRetrying()
    {
        RetryCount++;
        LastRetryAt = DateTime.UtcNow;
        Status = EmailStatus.Retrying;

        // Exponential backoff: 5min, 15min, 30min, 1hour, 2hours, etc.
        var delayMinutes = Math.Pow(2, RetryCount) * 5;
        NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
    }

    /// <summary>
    /// Marks the email as successfully sent.
    /// </summary>
    public void MarkAsSent()
    {
        Status = EmailStatus.Sent;
        NextRetryAt = null;
        LastError = null;
    }

    /// <summary>
    /// Marks the email as failed with an error message.
    /// </summary>
    public void MarkAsFailed(string error)
    {
        LastError = error;

        if (RetryCount >= MaxRetries)
        {
            Status = EmailStatus.Failed;
            NextRetryAt = null;
        }
        else
        {
            Status = EmailStatus.Pending;
        }
    }

    /// <summary>
    /// Checks if the email is ready to be retried.
    /// </summary>
    public bool IsReadyForRetry()
    {
        return Status == EmailStatus.Pending
            && NextRetryAt.HasValue
            && NextRetryAt.Value <= DateTime.UtcNow
            && RetryCount < MaxRetries;
    }
}

/// <summary>
/// Status of an email in the retry queue.
/// </summary>
public enum EmailStatus
{
    Pending = 0,
    Retrying = 1,
    Sent = 2,
    Failed = 3
}
