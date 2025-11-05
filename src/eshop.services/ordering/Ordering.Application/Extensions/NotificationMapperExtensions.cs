using BuildingBlocks.Messaging.Events;
using Ordering.Domain.Models;

namespace Ordering.Application.Extensions;

public static class NotificationMapperExtensions
{
    public static OrderNotificationDto ToOrderNotificationDto(this Order order)
    {
        return new OrderNotificationDto
        {
            Id = order.Id.Value,
            OrderName = order.OrderName.Value,
            OrderStatus = order.OrderStatus.ToString(),
            TotalPrice = order.OrderItems.Sum(oi => oi.Quantity * (oi.FinalPrice ?? oi.Price)),
            ShippingAddress = new ShippingAddressDto
            {
                FirstName = order.ShippingAddress.FirstName,
                LastName = order.ShippingAddress.LastName,
                AddressLine = order.ShippingAddress.AddressLine,
                Country = order.ShippingAddress.Country,
                State = order.ShippingAddress.State,
                ZipCode = order.ShippingAddress.ZipCode
            },
            Payment = new PaymentInfoDto
            {
                CardName = order.Payment.CardName ?? string.Empty,
                CardNumber = order.Payment.CardNumber,
                PaymentMethod = order.Payment.PaymentMethod.ToString()
            },
            OrderItems = order.OrderItems.Select(oi => new OrderItemNotificationDto
            {
                ProductId = oi.ProductId.Value,
                ProductName = oi.ProductName ?? $"Product {oi.ProductId.Value}",
                Quantity = oi.Quantity,
                Price = oi.FinalPrice ?? oi.Price,
                Discount = oi.DiscountAmount ?? 0
            }).ToList()
        };
    }

    public static OrderConfirmationNotificationEvent ToOrderConfirmationNotificationEvent(this Order order, string customerEmail)
    {
        return new OrderConfirmationNotificationEvent
        {
            CustomerEmail = customerEmail,
            Order = order.ToOrderNotificationDto()
        };
    }

    public static OrderStatusUpdateNotificationEvent ToOrderStatusUpdateNotificationEvent(this Order order, string customerEmail)
    {
        return new OrderStatusUpdateNotificationEvent
        {
            CustomerEmail = customerEmail,
            OrderId = order.Id.Value,
            OrderName = order.OrderName.Value,
            NewStatus = order.OrderStatus.ToString(),
            ShippingAddress = new ShippingAddressDto
            {
                FirstName = order.ShippingAddress.FirstName,
                LastName = order.ShippingAddress.LastName,
                AddressLine = order.ShippingAddress.AddressLine,
                Country = order.ShippingAddress.Country,
                State = order.ShippingAddress.State,
                ZipCode = order.ShippingAddress.ZipCode
            }
        };
    }

    public static OrderCancelledNotificationEvent ToOrderCancelledNotificationEvent(this Order order, string customerEmail)
    {
        return new OrderCancelledNotificationEvent
        {
            CustomerEmail = customerEmail,
            OrderId = order.Id.Value,
            OrderName = order.OrderName.Value,
            ShippingAddress = new ShippingAddressDto
            {
                FirstName = order.ShippingAddress.FirstName,
                LastName = order.ShippingAddress.LastName,
                AddressLine = order.ShippingAddress.AddressLine,
                Country = order.ShippingAddress.Country,
                State = order.ShippingAddress.State,
                ZipCode = order.ShippingAddress.ZipCode
            }
        };
    }
}
