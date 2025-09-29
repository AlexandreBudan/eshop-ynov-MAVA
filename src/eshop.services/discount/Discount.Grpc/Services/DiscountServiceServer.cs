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
public class DiscountServiceServer(DiscountContext dbContext, ILogger<DiscountServiceServer> logger) : DiscountProtoService.DiscountProtoServiceBase
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
        logger.LogInformation("Retrieving discount for {ProductName}", request.ProductName);
        
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.ProductName);
        
        if (coupon == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with name {request.ProductName} not found"));
        
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
                                                                      || x.Id == request.Coupon.Id);
        if(coupon is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with name {request.Coupon.ProductName} " +
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
                                                                      || x.Id == request.Coupon.Id);
        if(coupon is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Coupon with name {request.Coupon.ProductName} " +
                                                                   $" or Id {request.Coupon.Id} not found"));
        dbContext.Coupons.Remove(coupon);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Discount deleted for {ProductName}", coupon.ProductName);
        
        return new DeleteDiscountResponse(){Success = true};
    }
}