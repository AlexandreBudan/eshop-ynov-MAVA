using Discount.Grpc.Models;

namespace Discount.Grpc.Services;

/// <summary>
/// Service responsible for calculating discounts with business rules
/// </summary>
public class DiscountCalculator
{
    /// <summary>
    /// Calculates the total discount for a product given a list of coupons and the original price
    /// </summary>
    /// <param name="originalPrice">The original price of the product</param>
    /// <param name="coupons">List of coupons to apply</param>
    /// <param name="requestedCouponCodes">Optional coupon codes to apply</param>
    /// <param name="categories">Optional product categories for category-based discounts</param>
    /// <returns>A tuple containing the total discount amount, final price, and list of applied discounts</returns>
    public (double totalDiscount, double finalPrice, List<AppliedDiscountInfo> appliedDiscounts, string? warningMessage)
        CalculateDiscount(double originalPrice, List<Coupon> coupons, List<string>? requestedCouponCodes = null, List<string>? categories = null)
    {
        if (originalPrice <= 0)
            return (0, originalPrice, new List<AppliedDiscountInfo>(), "Invalid original price");

        if (coupons == null || !coupons.Any())
            return (0, originalPrice, new List<AppliedDiscountInfo>(), null);

        // Filter coupons: by code, validity, minimum amount, categories, and active status
        var applicableCoupons = FilterCoupons(coupons, requestedCouponCodes, originalPrice, categories);

        if (!applicableCoupons.Any())
            return (0, originalPrice, new List<AppliedDiscountInfo>(), "No applicable coupons found");

        // Sort by priority (higher first) and then by type (percentage first for better cumulative effect)
        var sortedCoupons = applicableCoupons
            .OrderByDescending(c => c.Priority)
            .ThenByDescending(c => c.Type == DiscountType.Percentage ? 1 : 0)
            .ToList();

        var appliedDiscounts = new List<AppliedDiscountInfo>();
        double currentPrice = originalPrice;
        double totalPercentageApplied = 0;
        string? warningMessage = null;

        // Check for non-stackable coupons
        var nonStackable = sortedCoupons.FirstOrDefault(c => !c.IsStackable);
        if (nonStackable != null)
        {
            if (sortedCoupons.Count > 1)
            {
                warningMessage = $"Only one discount applied: {nonStackable.Description} is not stackable with other discounts";
                sortedCoupons = new List<Coupon> { nonStackable };
            }
        }

        foreach (var coupon in sortedCoupons)
        {
            var discountAmount = CalculateSingleDiscount(currentPrice, coupon, ref totalPercentageApplied, out var percentageUsed, originalPrice);

            // Check if we exceeded the maximum stack percentage
            if (totalPercentageApplied > coupon.MaxStackPercentage)
            {
                warningMessage = $"Maximum stacking limit of {coupon.MaxStackPercentage}% reached. Some discounts were not fully applied.";

                // Recalculate with the remaining allowable percentage
                var remainingPercentage = coupon.MaxStackPercentage - (totalPercentageApplied - percentageUsed);
                if (remainingPercentage > 0 && coupon.Type != DiscountType.Fixed)
                {
                    // Apply only the remaining percentage
                    discountAmount = currentPrice * (remainingPercentage / 100.0);
                    totalPercentageApplied = coupon.MaxStackPercentage;
                }
                else
                {
                    // Skip this discount entirely if we've hit the limit
                    continue;
                }
            }

            // Ensure we don't discount more than the current price
            if (discountAmount > currentPrice)
            {
                warningMessage = $"Discount amount capped at current price to prevent negative values";
                discountAmount = currentPrice;
            }

            appliedDiscounts.Add(new AppliedDiscountInfo
            {
                Description = coupon.Description,
                DiscountAmount = discountAmount,
                Type = coupon.Type
            });

            currentPrice -= discountAmount;

            // Prevent negative prices
            if (currentPrice < 0)
            {
                currentPrice = 0;
                break;
            }
        }

        var totalDiscount = originalPrice - currentPrice;

        return (totalDiscount, currentPrice, appliedDiscounts, warningMessage);
    }

    /// <summary>
    /// Filters coupons by code, validity, status, minimum purchase amount, and categories
    /// </summary>
    private List<Coupon> FilterCoupons(List<Coupon> coupons, List<string>? requestedCodes, double purchaseAmount, List<string>? categories)
    {
        // First, filter by validity (dates, status, usage count)
        var validCoupons = coupons.Where(c => c.IsValid()).ToList();

        // Filter by minimum purchase amount
        validCoupons = validCoupons.Where(c => purchaseAmount >= c.MinimumPurchaseAmount).ToList();

        // Filter by category scope
        if (categories != null && categories.Any())
        {
            validCoupons = validCoupons.Where(c =>
            {
                // Global and Cart scopes always apply
                if (c.Scope == DiscountScope.Global || c.Scope == DiscountScope.Cart)
                    return true;

                // Product scope applies to specific products (already filtered by productId/name)
                if (c.Scope == DiscountScope.Product)
                    return true;

                // Category scope: check if product categories match applicable categories
                if (c.Scope == DiscountScope.Category)
                    return categories.Any(cat => c.IsApplicableToCategory(cat));

                return false;
            }).ToList();
        }

        // Then filter by coupon codes and automatic campaigns
        if (requestedCodes == null || !requestedCodes.Any())
        {
            // Return automatic discounts (no code required OR automatic campaigns)
            return validCoupons.Where(c => string.IsNullOrEmpty(c.CouponCode) || c.IsAutomatic).ToList();
        }

        var result = new List<Coupon>();

        // Add automatic discounts and campaigns (no code required)
        result.AddRange(validCoupons.Where(c => string.IsNullOrEmpty(c.CouponCode) || c.IsAutomatic));

        // Add coupons matching the requested codes
        foreach (var code in requestedCodes)
        {
            var matchingCoupons = validCoupons.Where(c =>
                !string.IsNullOrEmpty(c.CouponCode) &&
                c.CouponCode.Equals(code, StringComparison.OrdinalIgnoreCase)).ToList();

            result.AddRange(matchingCoupons);
        }

        return result.Distinct().ToList();
    }

    /// <summary>
    /// Calculates the discount amount for a single coupon (supports tiered discounts)
    /// </summary>
    private double CalculateSingleDiscount(double currentPrice, Coupon coupon, ref double totalPercentageApplied, out double percentageUsed, double originalPrice)
    {
        percentageUsed = 0;
        double discountAmount = 0;

        // Check if this is a tiered discount
        if (coupon.IsTiered)
        {
            var tier = coupon.GetApplicableTier(originalPrice);
            if (tier != null)
            {
                // Apply tier-specific discount
                if (tier.PercentageDiscount > 0)
                {
                    discountAmount = currentPrice * (tier.PercentageDiscount / 100.0);
                    percentageUsed = tier.PercentageDiscount;
                    totalPercentageApplied += tier.PercentageDiscount;
                }

                if (tier.FixedDiscount > 0)
                {
                    discountAmount += tier.FixedDiscount;
                }

                return discountAmount;
            }
        }

        // Standard discount calculation (non-tiered)
        switch (coupon.Type)
        {
            case DiscountType.Fixed:
                discountAmount = coupon.Amount;
                break;

            case DiscountType.Percentage:
                discountAmount = currentPrice * (coupon.PercentageDiscount / 100.0);
                percentageUsed = coupon.PercentageDiscount;
                totalPercentageApplied += coupon.PercentageDiscount;
                break;

            case DiscountType.Combined:
                // Apply percentage first, then fixed amount
                var percentageDiscount = currentPrice * (coupon.PercentageDiscount / 100.0);
                discountAmount = percentageDiscount + coupon.Amount;
                percentageUsed = coupon.PercentageDiscount;
                totalPercentageApplied += coupon.PercentageDiscount;
                break;
        }

        return discountAmount;
    }
}

/// <summary>
/// Information about an applied discount
/// </summary>
public class AppliedDiscountInfo
{
    public string Description { get; set; } = string.Empty;
    public double DiscountAmount { get; set; }
    public global::Discount.Grpc.DiscountType Type { get; set; }
}
