using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

/// <summary>
/// The DiscountServiceServer class implements the gRPC service for managing discount data.
/// It provides CRUD operations for discounts and communicates with the underlying database using a DbContext.
/// This class inherits from DiscountProtoServiceBase, which defines the service methods in the gRPC contract,
/// and implements the necessary logic for handling those methods.
/// </summary>
/// <remarks>
/// This class uses the DiscountContext for database interactions and ILogger for logging purposes.
/// It is registered with the gRPC pipeline in the application startup configuration.
/// </remarks>
public class DiscountServiceServer(DiscountContext dbContext, ILogger<DiscountServiceServer> logger, DiscountCalculator calculator) : DiscountProtoService.DiscountProtoServiceBase
{
    /// <summary>
    /// Retrieves discount details for a given product from the database.
    /// </summary>
    /// <param name="request">The request containing the product name to fetch the discount for.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns a <see cref="CouponModel"/> containing the discount details for the specified product.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if no discount is found for the specified product name.
    /// </exception>
    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        logger.LogInformation("Retrieving discount for ProductName: {ProductName}, ProductId: {ProductId}",
            request.ProductName, request.ProductId);

        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x =>
            x.ProductName == request.ProductName || x.ProductId == request.ProductId) ?? throw new RpcException(new Status(StatusCode.NotFound,
                $"Coupon with name {request.ProductName} or id {request.ProductId} not found"));
        logger.LogInformation("Discount retrieved for {ProductName}: {Amount}", coupon.ProductName, coupon.Amount);

        return coupon.Adapt<CouponModel>();
    }

    /// <summary>
    /// Creates a new discount for a specified product and stores it in the database.
    /// </summary>
    /// <param name="request">The request containing the details of the new discount to create, including the coupon information.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns a <see cref="CouponModel"/> representing the newly created discount.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if the request's coupon information is null.
    /// </exception>
    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        if (request.Coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is null"));
        
        var coupon = request.Coupon.Adapt<Coupon>();
        logger.LogInformation("Creating new discount for {ProductName}", coupon.ProductName);
        await dbContext.Coupons.AddAsync(coupon);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Discount created for {ProductName}: {Amount}", coupon.ProductName, coupon.Amount);
        return coupon.Adapt<CouponModel>();
    }

    /// <summary>
    /// Updates the discount details for a specific product based on the provided request.
    /// </summary>
    /// <param name="request">An object containing the updated discount information for a specific product.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns an updated <see cref="CouponModel"/> containing the modified discount details.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if the provided coupon is null, or if the specified product or coupon identifier is not found in the database.
    /// </exception>
    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        if (request.Coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is null"));
        
        logger.LogInformation("Updating discount for {ProductName}", request.Coupon.ProductName);

        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.Coupon.ProductName 
                                                                      || x.Id == request.Coupon.Id) ?? throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with name {request.Coupon.ProductName} " +
                                                                   $" or Id {request.Coupon.Id} not found"));
        request.Coupon.Adapt(coupon);
        
        dbContext.Coupons.Update(coupon);
        await dbContext.SaveChangesAsync();
        
        logger.LogInformation("Discount updated for {ProductName}: {Amount}", coupon.ProductName, coupon.Amount);
        return coupon.Adapt<CouponModel>();
    }

    /// <summary>
    /// Deletes a discount for a specified product based on the provided coupon details.
    /// </summary>
    /// <param name="request">The request containing the details of the coupon to be deleted, including the product name or ID.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>
    /// Returns a <see cref="DeleteDiscountResponse"/> indicating whether the discount was successfully deleted.
    /// </returns>
    /// <exception cref="RpcException">
    /// Thrown if the provided coupon is null, or if no matching discount is found for the specified product name or ID.
    /// </exception>
    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request,
        ServerCallContext context)
    {
        if (request.Coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Coupon is null"));

        logger.LogInformation("Deleting discount for {ProductName}", request.Coupon.ProductName);
        
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.Coupon.ProductName 
                                                                      || x.Id == request.Coupon.Id) ?? throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with name {request.Coupon.ProductName} " +
                                                                   $" or Id {request.Coupon.Id} not found"));
        dbContext.Coupons.Remove(coupon);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Discount deleted for {ProductName}", coupon.ProductName);
        
        return new DeleteDiscountResponse(){Success = true};
    }

    /// <summary>
    /// Retrieves all applicable discounts for a product (including code-based ones if codes are provided).
    /// </summary>
    /// <param name="request">The request containing product information and optional coupon codes.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>Returns a list of applicable coupons.</returns>
    public override async Task<CouponListModel> GetDiscountsForProduct(GetDiscountRequest request, ServerCallContext context)
    {
        logger.LogInformation("Retrieving all discounts for ProductName: {ProductName}, ProductId: {ProductId}",
            request.ProductName, request.ProductId);

        var query = dbContext.Coupons.AsQueryable();

        // Filter by product
        query = query.Where(x => x.ProductName == request.ProductName || x.ProductId == request.ProductId);

        var coupons = await query.ToListAsync(context.CancellationToken);

        var couponListModel = new CouponListModel();
        couponListModel.Coupons.AddRange(coupons.Select(c => c.Adapt<CouponModel>()));

        logger.LogInformation("Retrieved {Count} discounts for product", couponListModel.Coupons.Count);

        return couponListModel;
    }

    /// <summary>
    /// Calculates the total discount for a product with business logic for stacking and validation.
    /// </summary>
    /// <param name="request">The request containing product information, original price, and optional coupon codes.</param>
    /// <param name="context">The gRPC server call context.</param>
    /// <returns>Returns the calculated discount with final price and applied discounts.</returns>
    public override async Task<CalculateDiscountResponse> CalculateDiscount(CalculateDiscountRequest request, ServerCallContext context)
    {
        logger.LogInformation("Calculating discount for ProductName: {ProductName}, ProductId: {ProductId}, Price: {Price}",
            request.ProductName, request.ProductId, request.OriginalPrice);

        // Get all applicable coupons for this product
        var query = dbContext.Coupons.AsQueryable();
        query = query.Where(x => x.ProductName == request.ProductName || x.ProductId == request.ProductId);
        var coupons = await query.ToListAsync(context.CancellationToken);

        if (!coupons.Any())
        {
            logger.LogInformation("No discounts found for product");
            return new CalculateDiscountResponse
            {
                TotalDiscount = 0,
                FinalPrice = request.OriginalPrice,
                HasWarning = false
            };
        }

        // Use the calculator to compute discounts with business rules
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            calculator.CalculateDiscount(request.OriginalPrice, coupons, request.CouponCodes?.ToList(), request.Categories?.ToList());

        var response = new CalculateDiscountResponse
        {
            TotalDiscount = totalDiscount,
            FinalPrice = finalPrice,
            HasWarning = !string.IsNullOrEmpty(warningMessage),
            WarningMessage = warningMessage ?? string.Empty
        };

        foreach (var discount in appliedDiscounts)
        {
            response.AppliedDiscounts.Add(new AppliedDiscount
            {
                Description = discount.Description,
                DiscountAmount = discount.DiscountAmount,
                Type = (DiscountType)discount.Type
            });
        }

        logger.LogInformation("Calculated discount: Total={TotalDiscount}, Final={FinalPrice}, Applied={Count}",
            totalDiscount, finalPrice, appliedDiscounts.Count);

        return response;
    }
}