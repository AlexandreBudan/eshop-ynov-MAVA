using BuildingBlocks.Messaging.Events;
using Microsoft.Extensions.Options;
using Notification.API.Models;
using System.Net;
using System.Net.Mail;

namespace Notification.API.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendOrderConfirmationEmailAsync(OrderNotificationDto order, string customerEmail, CancellationToken cancellationToken = default)
    {
        try
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = $"Order Confirmation - {order.OrderName}",
                Body = BuildOrderConfirmationEmailBody(order),
                IsBodyHtml = true
            };

            mailMessage.To.Add(customerEmail);

            await SendEmailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Order confirmation email sent successfully to {CustomerEmail} for order {OrderId}",
                customerEmail, order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email to {CustomerEmail} for order {OrderId}",
                customerEmail, order.Id);
            throw;
        }
    }

    public async Task SendOrderStatusUpdateEmailAsync(Guid orderId, string orderName, string newStatus,
        ShippingAddressDto shippingAddress, string customerEmail, CancellationToken cancellationToken = default)
    {
        try
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = $"Order Status Update - {orderName}",
                Body = BuildOrderStatusUpdateEmailBody(orderId, orderName, newStatus, shippingAddress),
                IsBodyHtml = true
            };

            mailMessage.To.Add(customerEmail);

            await SendEmailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Order status update email sent successfully to {CustomerEmail} for order {OrderId}",
                customerEmail, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order status update email to {CustomerEmail} for order {OrderId}",
                customerEmail, orderId);
            throw;
        }
    }

    public async Task SendOrderCancelledEmailAsync(Guid orderId, string orderName,
        ShippingAddressDto shippingAddress, string customerEmail, CancellationToken cancellationToken = default)
    {
        try
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = $"Order Cancellation - {orderName}",
                Body = BuildOrderCancelledEmailBody(orderId, orderName, shippingAddress),
                IsBodyHtml = true
            };

            mailMessage.To.Add(customerEmail);

            await SendEmailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Order cancellation email sent successfully to {CustomerEmail} for order {OrderId}",
                customerEmail, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order cancellation email to {CustomerEmail} for order {OrderId}",
                customerEmail, orderId);
            throw;
        }
    }

    private async Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken)
    {
        using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            EnableSsl = _emailSettings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = string.IsNullOrEmpty(_emailSettings.SmtpUsername)
                ? null
                : new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
        };

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }

    private string BuildOrderConfirmationEmailBody(OrderNotificationDto order)
    {
        var itemsHtml = string.Join("", order.OrderItems.Select(item =>
        {
            var hasDiscount = item.Discount > 0;
            var itemSubtotal = item.Quantity * item.Price;

            var discountBadge = hasDiscount
                ? $"<span style='color: #4CAF50; font-weight: bold;'>-${item.Discount:F2} discount</span>"
                : "";

            return $@"<tr>
                <td style='padding: 8px; border-bottom: 1px solid #ddd;'>
                    <strong>{item.ProductName}</strong><br>
                    {(hasDiscount ? $"<small style='color: #666;'>{discountBadge}</small>" : "")}
                </td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: right;'>${item.Price:F2}</td>
                <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: right;'><strong>${itemSubtotal:F2}</strong></td>
            </tr>";
        }));

        var totalDiscount = order.OrderItems.Sum(item => item.Discount);
        var hasAnyDiscount = totalDiscount > 0;

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
                        <th style='text-align: left;'>Product</th>
                        <th style='text-align: center;'>Quantity</th>
                        <th style='text-align: right;'>Unit Price</th>
                        <th style='text-align: right;'>Subtotal</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
            </table>

            {(hasAnyDiscount ? $@"
            <div style='text-align: right; padding: 10px 0; color: #4CAF50; font-size: 1.1em;'>
                <strong>Total Savings: ${totalDiscount:F2}</strong>
            </div>" : "")}

            <div class='total'>
                <span style='color: #666; font-size: 0.9em;'>Order Total:</span><br>
                ${order.TotalPrice:F2}
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

    private string BuildOrderStatusUpdateEmailBody(Guid orderId, string orderName, string newStatus, ShippingAddressDto shippingAddress)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #FFA500; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; color: #777; font-size: 0.9em; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Status Update</h1>
        </div>
        <div class='content'>
            <h2>Your Order Status has Changed</h2>
            <p>The status of your order <strong>{orderName}</strong> has been updated to <strong>{newStatus}</strong>.</p>

            <h3>Order Details:</h3>
            <p><strong>Order ID:</strong> {orderId}</p>

            <h3>Shipping Address:</h3>
            <p>
                {shippingAddress.FirstName} {shippingAddress.LastName}<br>
                {shippingAddress.AddressLine}<br>
                {shippingAddress.State} {shippingAddress.ZipCode}<br>
                {shippingAddress.Country}
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

    private string BuildOrderCancelledEmailBody(Guid orderId, string orderName, ShippingAddressDto shippingAddress)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #FF0000; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; color: #777; font-size: 0.9em; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Cancellation</h1>
        </div>
        <div class='content'>
            <h2>Your Order has been Cancelled</h2>
            <p>Your order <strong>{orderName}</strong> has been cancelled.</p>

            <h3>Order Details:</h3>
            <p><strong>Order ID:</strong> {orderId}</p>

            <h3>Shipping Address:</h3>
            <p>
                {shippingAddress.FirstName} {shippingAddress.LastName}<br>
                {shippingAddress.AddressLine}<br>
                {shippingAddress.State} {shippingAddress.ZipCode}<br>
                {shippingAddress.Country}
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
