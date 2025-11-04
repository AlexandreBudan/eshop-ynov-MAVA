using Ordering.Domain.Enums;

namespace Ordering.Application.Features.Orders.Dtos;

public record OrderDto(Guid Id,
    Guid CustomerId, 
    String OrderName, 
    AddressDto ShippingAddress, 
    AddressDto  BillingAddress,
    PaymentDto Payment,
    OrderStatus OrderStatus,
    List<OrderItemDto> OrderItems);