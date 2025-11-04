using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Application.Services;
using System.Text.Json;

namespace Ordering.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically processes the failed email queue and retries sending.
/// </summary>
public class EmailRetryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailRetryBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(2); // Check every 2 minutes

    public EmailRetryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<EmailRetryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Retry Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessFailedEmailsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing failed emails");
            }

            // Wait for the next check interval
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Email Retry Background Service stopped");
    }

    private async Task ProcessFailedEmailsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailRetryService = scope.ServiceProvider.GetRequiredService<IEmailRetryService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Get emails ready for retry
        var emailsToRetry = await emailRetryService.GetEmailsReadyForRetryAsync(
            batchSize: 10,
            cancellationToken: cancellationToken);

        if (emailsToRetry.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} emails for retry", emailsToRetry.Count);

        foreach (var (failedEmailId, orderId, recipientEmail, emailSubject, emailBody) in emailsToRetry)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                _logger.LogInformation("Retrying email {FailedEmailId} for order {OrderId} to {RecipientEmail}",
                    failedEmailId, orderId, recipientEmail);

                // Deserialize the order DTO from the stored JSON
                var order = JsonSerializer.Deserialize<OrderDto>(emailBody);

                if (order == null)
                {
                    _logger.LogError("Failed to deserialize order DTO for email {FailedEmailId}", failedEmailId);
                    await emailRetryService.MarkEmailAsFailedAsync(
                        failedEmailId,
                        "Failed to deserialize order data",
                        cancellationToken);
                    continue;
                }

                // Attempt to send the email
                await emailService.SendOrderConfirmationEmailAsync(order, recipientEmail, cancellationToken);

                // Mark as successfully sent
                await emailRetryService.MarkEmailAsSentAsync(failedEmailId, cancellationToken);

                _logger.LogInformation("Successfully retried and sent email {FailedEmailId}", failedEmailId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retry email {FailedEmailId}: {Error}",
                    failedEmailId, ex.Message);

                // Mark as failed and schedule next retry
                await emailRetryService.MarkEmailAsFailedAsync(
                    failedEmailId,
                    ex.Message,
                    cancellationToken);
            }
        }

        _logger.LogInformation("Finished processing email retry batch");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Retry Background Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
