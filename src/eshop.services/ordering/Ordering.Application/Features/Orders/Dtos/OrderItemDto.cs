namespace Ordering.Application.Features.Orders.Dtos;

public record OrderItemDto(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    decimal Price,
    string? ProductName = null,
    string? ProductDescription = null,
    string? ImageFile = null,
    decimal? DiscountAmount = null,
    decimal? FinalPrice = null);