using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Services;

namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IOrderingDbContext orderingDbContext,
    ICatalogService catalogService,
    ILogger<CreateOrderCommandHandler> logger) : ICommandHandler<CreateOrderCommand, CreateOrderCommandResult>
{
    /// <summary>
    /// Handles the execution logic for creating an order command.
    /// Validates product availability with Catalog.API and enriches order items with product details.
    /// </summary>
    /// <param name="request">The create order command containing the order details.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the result of the handling operation, containing the newly created order's ID.</returns>
    public async Task<CreateOrderCommandResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating order for customer {CustomerId} with {ItemCount} items",
            request.Order.CustomerId, request.Order.OrderItems.Count);

        var productIds = request.Order.OrderItems.Select(i => i.ProductId.ToString()).ToList();
        try
        {
            var productsAvailable = await catalogService.ValidateProductsAvailabilityAsync(productIds, cancellationToken);

            if (!productsAvailable)
            {
                logger.LogWarning("Some products are not available in catalog. Proceeding anyway for resilience.");
            }
            else
            {
                logger.LogInformation("All products validated successfully from Catalog.API");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to validate products with Catalog service. Proceeding without validation for resilience.");
        }

        var enrichedOrderItems = new List<Ordering.Application.Features.Orders.Dtos.OrderItemDto>();
        foreach (var item in request.Order.OrderItems)
        {
            try
            {
                var productInfo = await catalogService.GetProductAsync(item.ProductId.ToString(), cancellationToken);

                if (productInfo != null)
                {
                    var enrichedItem = new Ordering.Application.Features.Orders.Dtos.OrderItemDto(
                        OrderId: item.OrderId,
                        ProductId: item.ProductId,
                        Quantity: item.Quantity,
                        Price: productInfo.FinalPrice,
                        ProductName: productInfo.Name,
                        ProductDescription: productInfo.Description,
                        ImageFile: productInfo.ImageFile,
                        DiscountAmount: productInfo.HasDiscount ? productInfo.DiscountAmount : null,
                        FinalPrice: productInfo.FinalPrice
                    );

                    enrichedOrderItems.Add(enrichedItem);
                    logger.LogInformation("Enriched order item for product {ProductId}: {ProductName} - Price: {Price} (Discount: {Discount})",
                        productInfo.Id, productInfo.Name, productInfo.FinalPrice, productInfo.DiscountAmount);
                }
                else
                {
                    enrichedOrderItems.Add(item);
                    logger.LogWarning("Could not enrich product {ProductId}, using original data", item.ProductId);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch product details for {ProductId}, using original data", item.ProductId);
                enrichedOrderItems.Add(item);
            }
        }

        var enrichedOrder = request.Order with { OrderItems = enrichedOrderItems };
        var order = CreateOrderCommandMapper.CreateNewOrderFromDto(enrichedOrder);
        orderingDbContext.Orders.Add(order);
        await orderingDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Order {OrderId} created successfully with enriched product details", order.Id.Value);
        return new CreateOrderCommandResult(order.Id.Value);
    }
}