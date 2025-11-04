using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Application.Services;
using Ordering.Domain.Models;
using System.Text.Json;

namespace Ordering.Infrastructure.Services;

/// <summary>
/// Service for managing email retry queue.
/// </summary>
public class EmailRetryService : IEmailRetryService
{
    private readonly IOrderingDbContext _dbContext;
    private readonly ILogger<EmailRetryService> _logger;

    public EmailRetryService(IOrderingDbContext dbContext, ILogger<EmailRetryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task QueueFailedEmailAsync(
        Guid orderId,
        string recipientEmail,
        OrderDto order,
        string error,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Queueing failed email for order {OrderId} to {RecipientEmail}", orderId, recipientEmail);

        try
        {
            // Serialize the order DTO to JSON for storage
            var emailBody = JsonSerializer.Serialize(order, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            var failedEmail = FailedEmail.Create(
                orderId,
                recipientEmail,
                $"Order Confirmation - {order.OrderName}",
                emailBody,
                error,
                maxRetries: 5 // Allow up to 5 retries
            );

            _dbContext.FailedEmails.Add(failedEmail);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Failed email queued successfully with ID {FailedEmailId}. Will retry in 5 minutes.", failedEmail.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue email for retry for order {OrderId}", orderId);
            // Don't throw - we don't want to break the flow if queuing fails
        }
    }

    public async Task<List<(Guid FailedEmailId, Guid OrderId, string RecipientEmail, string EmailSubject, string EmailBody)>> GetEmailsReadyForRetryAsync(
        int batchSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var readyEmails = await _dbContext.FailedEmails
                .Where(e => e.Status == EmailStatus.Pending
                    && e.NextRetryAt.HasValue
                    && e.NextRetryAt.Value <= DateTime.UtcNow
                    && e.RetryCount < e.MaxRetries)
                .OrderBy(e => e.NextRetryAt)
                .Take(batchSize)
                .Select(e => new
                {
                    e.Id,
                    e.OrderId,
                    e.RecipientEmail,
                    e.EmailSubject,
                    e.EmailBody
                })
                .ToListAsync(cancellationToken);

            var result = readyEmails.Select(e => (
                e.Id,
                e.OrderId,
                e.RecipientEmail,
                e.EmailSubject,
                e.EmailBody
            )).ToList();

            if (result.Count > 0)
            {
                _logger.LogInformation("Found {Count} emails ready for retry", result.Count);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve emails ready for retry");
            return new List<(Guid, Guid, string, string, string)>();
        }
    }

    public async Task MarkEmailAsSentAsync(Guid failedEmailId, CancellationToken cancellationToken = default)
    {
        try
        {
            var failedEmail = await _dbContext.FailedEmails.FindAsync(new object[] { failedEmailId }, cancellationToken);

            if (failedEmail != null)
            {
                failedEmail.MarkAsSent();
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Email {FailedEmailId} marked as sent after {RetryCount} retries",
                    failedEmailId, failedEmail.RetryCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark email {FailedEmailId} as sent", failedEmailId);
        }
    }

    public async Task MarkEmailAsFailedAsync(Guid failedEmailId, string error, CancellationToken cancellationToken = default)
    {
        try
        {
            var failedEmail = await _dbContext.FailedEmails.FindAsync(new object[] { failedEmailId }, cancellationToken);

            if (failedEmail != null)
            {
                failedEmail.MarkAsRetrying();
                failedEmail.MarkAsFailed(error);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (failedEmail.Status == EmailStatus.Failed)
                {
                    _logger.LogWarning("Email {FailedEmailId} permanently failed after {RetryCount} retries. Error: {Error}",
                        failedEmailId, failedEmail.RetryCount, error);
                }
                else
                {
                    _logger.LogInformation("Email {FailedEmailId} marked as failed. Will retry at {NextRetryAt}",
                        failedEmailId, failedEmail.NextRetryAt);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark email {FailedEmailId} as failed", failedEmailId);
        }
    }
}
