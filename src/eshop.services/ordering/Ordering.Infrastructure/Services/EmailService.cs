using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Application.Services;
using System.Net;
using System.Net.Mail;

namespace Ordering.Infrastructure.Services;

/// <summary>
/// Email service implementation using SMTP.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Sends an order confirmation email to the customer.
    /// </summary>
    public async Task SendOrderConfirmationEmailAsync(OrderDto order, string customerEmail, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@eshop.com";
            var fromName = _configuration["EmailSettings:FromName"] ?? "eShop Support";
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "localhost";
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = $"Order Confirmation - {order.OrderName}",
                Body = BuildOrderConfirmationEmailBody(order),
                IsBodyHtml = true
            };

            mailMessage.To.Add(customerEmail);

            using var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Order confirmation email sent successfully to {CustomerEmail} for order {OrderId}",
                customerEmail, order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email to {CustomerEmail} for order {OrderId}",
                customerEmail, order.Id);

            // Note: We don't throw here to prevent email failures from breaking the order creation flow
            // In a production system, you might want to queue failed emails for retry
        }
    }

    private string BuildOrderConfirmationEmailBody(OrderDto order)
    {
        var itemsHtml = string.Join("", order.OrderItems.Select(item =>
            $@"<tr>
                <td style='padding: 8px; border-bottom: 1px solid #ddd;'>Product {item.ProductId}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{item.Quantity}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd;'>${item.Price:F2}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd;'>${(item.Quantity * item.Price):F2}</td>
            </tr>"));

        var totalPrice = order.OrderItems.Sum(item => item.Quantity * item.Price);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; background-color: white; }}
        th {{ background-color: #4CAF50; color: white; padding: 12px; text-align: left; }}
        .total {{ font-weight: bold; font-size: 1.2em; text-align: right; padding: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #777; font-size: 0.9em; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Confirmation</h1>
        </div>
        <div class='content'>
            <h2>Thank you for your order!</h2>
            <p>Your order <strong>{order.OrderName}</strong> has been confirmed.</p>

            <h3>Order Details:</h3>
            <p><strong>Order ID:</strong> {order.Id}</p>
            <p><strong>Status:</strong> {order.OrderStatus}</p>

            <h3>Shipping Address:</h3>
            <p>
                {order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}<br>
                {order.ShippingAddress.AddressLine}<br>
                {order.ShippingAddress.State} {order.ShippingAddress.ZipCode}<br>
                {order.ShippingAddress.Country}
            </p>

            <h3>Order Items:</h3>
            <table>
                <thead>
                    <tr>
                        <th>Product</th>
                        <th>Quantity</th>
                        <th>Price</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
            </table>

            <div class='total'>
                Total: ${totalPrice:F2}
            </div>

            <h3>Payment Information:</h3>
            <p>
                <strong>Card:</strong> **** **** **** {(order.Payment.CardNumber.Length >= 4 ? order.Payment.CardNumber.Substring(order.Payment.CardNumber.Length - 4) : order.Payment.CardNumber)}<br>
                <strong>Method:</strong> {order.Payment.PaymentMethod}
            </p>
        </div>
        <div class='footer'>
            <p>Thank you for shopping with us!<br>
            If you have any questions, please contact our support team.</p>
        </div>
    </div>
</body>
</html>";
    }
}
