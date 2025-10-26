namespace Ordering.Application.Features.Orders.Dtos;

public record OrderItemDto(Guid OrderId, Guid ProductId, int Quantity, decimal Price);