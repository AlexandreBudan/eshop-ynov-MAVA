using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

public record DeleteBasketItemCommand(string UserName, Guid ProductId) : ICommand<DeleteBasketItemResult>;

public record DeleteBasketItemResult(bool IsSuccess);
