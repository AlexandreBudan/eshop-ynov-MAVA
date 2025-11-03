namespace Discount.Grpc.Models;

public enum DiscountScope
{
    /// <summary>
    /// Discount applies to a specific product
    /// </summary>
    Product = 0,

    /// <summary>
    /// Discount applies to a product category
    /// </summary>
    Category = 1,

    /// <summary>
    /// Discount applies to the entire cart
    /// </summary>
    Cart = 2,

    /// <summary>
    /// Global discount for all products
    /// </summary>
    Global = 3
}

public enum CampaignType
{
    /// <summary>
    /// No specific campaign
    /// </summary>
    None = 0,

    /// <summary>
    /// Seasonal campaign (e.g., Black Friday)
    /// </summary>
    Seasonal = 1,

    /// <summary>
    /// Flash sale with a limited duration
    /// </summary>
    FlashSale = 2,

    /// <summary>
    /// Buy-One-Get-One-Free (BOGO)
    /// </summary>
    BOGO = 3
}

public class DiscountTier
{
    public int Id { get; set; }
    public int CouponId { get; set; }
    public double MinAmount { get; set; }
    public double PercentageDiscount { get; set; }
    public double FixedDiscount { get; set; }
    public int Order { get; set; }

    public bool IsApplicable(double amount)
    {
        return amount >= MinAmount;
    }
}