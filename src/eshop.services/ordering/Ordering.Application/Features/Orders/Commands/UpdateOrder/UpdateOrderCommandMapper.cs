using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrder;

public static class UpdateOrderCommandMapper
{
    /// <summary>
    /// Updates an existing order object with new values provided by a DTO.
    /// </summary>
    /// <param name="order">The existing order to be updated.</param>
    /// <param name="newOrderDto">The DTO containing the new values to update the order with.</param>
    public static void  UpdateOrderWithNewValues(Order order, OrderDto newOrderDto)
    {
        var updatedShippingAddress = Address.Of(newOrderDto.ShippingAddress.FirstName, newOrderDto.ShippingAddress.LastName, newOrderDto.ShippingAddress.EmailAddress,
            newOrderDto.ShippingAddress.AddressLine, newOrderDto.ShippingAddress.Country, newOrderDto.ShippingAddress.State, newOrderDto.ShippingAddress.ZipCode);
        var  updatedBillingAddress = Address.Of(newOrderDto.BillingAddress.FirstName, newOrderDto.BillingAddress.LastName, newOrderDto.BillingAddress.EmailAddress, newOrderDto.BillingAddress.AddressLine,
            newOrderDto.BillingAddress.Country, newOrderDto.BillingAddress.State, newOrderDto.BillingAddress.ZipCode);
        var  updatedPayment = Payment.Of(newOrderDto.Payment.CardName, newOrderDto.Payment.CardNumber, newOrderDto.Payment.Expiration, newOrderDto.Payment.Cvv, newOrderDto.Payment.PaymentMethod);

        order.Update(OrderName.Of(newOrderDto.OrderName),
            shippingAddress: updatedShippingAddress,
            billingAddress: updatedBillingAddress,
            payment: updatedPayment,
            orderStatus: newOrderDto.OrderStatus
            );
    }
}