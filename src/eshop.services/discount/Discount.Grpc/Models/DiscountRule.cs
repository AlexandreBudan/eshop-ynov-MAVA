namespace Discount.Grpc.Models;

/// <summary>
/// Represents a discount tier for progressive discounts based on purchase amount
/// </summary>
public class DiscountTier
{
    public int Id { get; set; }

    /// <summary>
    /// The coupon this tier belongs to
    /// </summary>
    public int CouponId { get; set; }

    /// <summary>
    /// Minimum purchase amount for this tier (inclusive)
    /// </summary>
    public double MinAmount { get; set; }

    /// <summary>
    /// Maximum purchase amount for this tier (exclusive, null = no limit)
    /// </summary>
    public double? MaxAmount { get; set; }

    /// <summary>
    /// Discount percentage for this tier
    /// </summary>
    public double PercentageDiscount { get; set; }

    /// <summary>
    /// Fixed discount amount for this tier
    /// </summary>
    public double FixedDiscount { get; set; }

    /// <summary>
    /// Priority order for applying tiers (lower = applied first)
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Checks if a purchase amount falls within this tier
    /// </summary>
    public bool IsApplicable(double amount)
    {
        if (amount < MinAmount)
            return false;

        if (MaxAmount.HasValue && amount >= MaxAmount.Value)
            return false;

        return true;
    }
}

/// <summary>
/// Scope of discount application
/// </summary>
public enum DiscountScope
{
    /// <summary>
    /// Discount applies to specific products only
    /// </summary>
    Product = 0,

    /// <summary>
    /// Discount applies to entire categories
    /// </summary>
    Category = 1,

    /// <summary>
    /// Discount applies to entire cart/basket
    /// </summary>
    Cart = 2,

    /// <summary>
    /// Discount applies globally to all items
    /// </summary>
    Global = 3
}

/// <summary>
/// Type of automatic campaign
/// </summary>
public enum CampaignType
{
    /// <summary>
    /// No automatic campaign
    /// </summary>
    None = 0,

    /// <summary>
    /// Black Friday campaign
    /// </summary>
    BlackFriday = 1,

    /// <summary>
    /// Seasonal sales
    /// </summary>
    SeasonalSales = 2,

    /// <summary>
    /// Flash sale
    /// </summary>
    FlashSale = 3,

    /// <summary>
    /// Holiday special
    /// </summary>
    Holiday = 4,

    /// <summary>
    /// Clearance sale
    /// </summary>
    Clearance = 5,

    /// <summary>
    /// Custom campaign
    /// </summary>
    Custom = 99
}
