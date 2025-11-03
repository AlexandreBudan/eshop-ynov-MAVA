using BuildingBlocks.Messaging.Events;
using Ordering.Application.Features.Orders.Commands.CreateOrder;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Models;

namespace Ordering.Application.Extensions;

public static class OrderMapperExtensions
{
    /// <summary>
    /// Maps a list of <see cref="Order"/> objects to a collection of <see cref="OrderDto"/> objects.
    /// </summary>
    /// <param name="orders">The list of <see cref="Order"/> objects to be mapped.</param>
    /// <returns>An enumerable collection of <see cref="OrderDto"/> objects.</returns>
    public static IEnumerable<OrderDto> ToOrderDtoList(this List<Order> orders)
    {
        return orders.Select(order => order.ToOrderDto());
    }

    /// <summary>
    /// Maps an <see cref="Order"/> object to an <see cref="OrderDto"/> object.
    /// </summary>
    /// <param name="order">The <see cref="Order"/> instance to be mapped.</param>
    /// <returns>An instance of <see cref="OrderDto"/> that corresponds to the given <see cref="Order"/>.</returns>
    public static OrderDto ToOrderDto(this Order order)
    {
        return DtoFromOrder(order);
    }
    
    /// <summary>
    /// Maps a <see cref="BasketCheckoutEvent"/> to a <see cref="CreateOrderCommand"/>.
    /// </summary>
    /// <param name="message">The <see cref="BasketCheckoutEvent"/> containing the basket checkout details.</param>
    /// <returns>A <see cref="CreateOrderCommand"/> object created using the details from the provided checkout event.</returns>
    public static CreateOrderCommand MapToCreateOrderCommand(BasketCheckoutEvent message)
    {
        // Create full order with incoming event data
        var addressDto = new AddressDto(message.FirstName, message.LastName, message.EmailAddress, message.AddressLine, message.Country, message.State, message.ZipCode);
        var paymentDto = new PaymentDto(message.CardName, message.CardNumber, message.Expiration, message.Cvv, message.PaymentMethod);
        var orderId = Guid.NewGuid();

        // Map basket items to order items
        var orderItems = message.Items.Select(item =>
            new OrderItemDto(orderId, item.ProductId, item.Quantity, item.Price)).ToList();

        var orderDto = new OrderDto(
            Id: orderId,
            CustomerId: message.CustomerId,
            OrderName: $"{message.UserName} Order",
            ShippingAddress: addressDto,
            BillingAddress: addressDto,
            Payment: paymentDto,
            OrderStatus: Ordering.Domain.Enums.OrderStatus.Pending,
            OrderItems: orderItems);

        return new CreateOrderCommand(orderDto);
    }

    /// <summary>
    /// Maps an <see cref="Order"/> object to an <see cref="OrderDto"/> object.
    /// </summary>
    /// <param name="order">The <see cref="Order"/> object to be mapped.</param>
    /// <returns>A mapped <see cref="OrderDto"/> object containing order details.</returns>
    private static OrderDto DtoFromOrder(Order order)
    {
        return new OrderDto(
            Id: order.Id.Value,
            CustomerId: order.CustomerId.Value,
            OrderName: order.OrderName.Value,
            ShippingAddress: new AddressDto(order.ShippingAddress.FirstName, order.ShippingAddress.LastName,
                order.ShippingAddress.EmailAddress!, order.ShippingAddress.AddressLine, order.ShippingAddress.Country,
                order.ShippingAddress.State, order.ShippingAddress.ZipCode),
            BillingAddress: new AddressDto(order.BillingAddress.FirstName, order.BillingAddress.LastName,
                order.BillingAddress.EmailAddress!, order.BillingAddress.AddressLine, order.BillingAddress.Country,
                order.BillingAddress.State, order.BillingAddress.ZipCode),
            Payment: new PaymentDto(order.Payment.CardName!, order.Payment.CardNumber, order.Payment.Expiration,
                order.Payment.CVV, order.Payment.PaymentMethod),
            OrderStatus: order.OrderStatus,
            OrderItems: order.OrderItems.Select(oi =>
                new OrderItemDto(
                    OrderId: oi.OrderId.Value,
                    ProductId: oi.ProductId.Value,
                    Quantity: oi.Quantity,
                    Price: oi.Price,
                    ProductName: oi.ProductName,
                    ProductDescription: oi.ProductDescription,
                    ImageFile: oi.ImageFile,
                    DiscountAmount: oi.DiscountAmount,
                    FinalPrice: oi.FinalPrice
                )).ToList()
        );
    }
    
}