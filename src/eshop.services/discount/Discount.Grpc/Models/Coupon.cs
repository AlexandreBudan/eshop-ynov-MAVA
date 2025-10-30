namespace Discount.Grpc.Models;

public class Coupon
{
    public int Id { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Fixed amount discount (e.g., 5€ off)
    /// </summary>
    public double Amount { get; set; }

    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Discount type: Percentage, Fixed, or Combined (uses gRPC generated enum)
    /// </summary>
    public DiscountType Type { get; set; } = DiscountType.Fixed;

    /// <summary>
    /// Percentage discount (e.g., 10 for 10%)
    /// </summary>
    public double PercentageDiscount { get; set; }

    /// <summary>
    /// Coupon code required to apply this discount (optional)
    /// </summary>
    public string? CouponCode { get; set; }

    /// <summary>
    /// Whether this discount can be stacked with others
    /// </summary>
    public bool IsStackable { get; set; } = true;

    /// <summary>
    /// Maximum total discount percentage when stacking (e.g., 30 for 30%)
    /// </summary>
    public double MaxStackPercentage { get; set; } = 30.0;

    /// <summary>
    /// Priority for applying discounts (higher = applied first)
    /// </summary>
    public int Priority { get; set; } = 0;

    // ===== Lifecycle Management Fields =====

    /// <summary>
    /// Coupon status: Active, Expired, Disabled, or UpcomingActive
    /// </summary>
    public CouponStatus Status { get; set; } = CouponStatus.Active;

    // ===== Contextual Rules Fields =====

    /// <summary>
    /// Scope of discount application (Product, Category, Cart, Global)
    /// </summary>
    public DiscountScope Scope { get; set; } = DiscountScope.Product;

    /// <summary>
    /// Applicable category names (comma-separated) for Category scope
    /// </summary>
    public string? ApplicableCategories { get; set; }

    /// <summary>
    /// Type of automatic campaign (None if manual code)
    /// </summary>
    public CampaignType CampaignType { get; set; } = CampaignType.None;

    /// <summary>
    /// Whether this is a tiered discount with multiple levels
    /// </summary>
    public bool IsTiered { get; set; } = false;

    /// <summary>
    /// Discount tiers for progressive discounts
    /// </summary>
    public List<DiscountTier> Tiers { get; set; } = new();

    /// <summary>
    /// Whether this discount is automatically applied (no code required)
    /// </summary>
    public bool IsAutomatic { get; set; } = false;

    /// <summary>
    /// Start date for the coupon validity period (null = no start date)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for the coupon validity period (null = no end date)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Minimum purchase amount required to use this coupon (0 = no minimum)
    /// </summary>
    public double MinimumPurchaseAmount { get; set; } = 0;

    /// <summary>
    /// Maximum number of times this coupon can be used (0 = unlimited)
    /// </summary>
    public int MaxUsageCount { get; set; } = 0;

    /// <summary>
    /// Current usage count
    /// </summary>
    public int CurrentUsageCount { get; set; } = 0;

    /// <summary>
    /// Date when the coupon was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the coupon was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Checks if the coupon is currently valid based on dates and status
    /// </summary>
    public bool IsValid()
    {
        var now = DateTime.UtcNow;

        // Check status
        if (Status != CouponStatus.Active)
            return false;

        // Check start date
        if (StartDate.HasValue && now < StartDate.Value)
            return false;

        // Check end date
        if (EndDate.HasValue && now > EndDate.Value)
            return false;

        // Check usage limit
        if (MaxUsageCount > 0 && CurrentUsageCount >= MaxUsageCount)
            return false;

        return true;
    }

    /// <summary>
    /// Updates the status based on current date and conditions
    /// </summary>
    public void UpdateStatus()
    {
        var now = DateTime.UtcNow;

        // Don't change if manually disabled
        if (Status == CouponStatus.Disabled)
            return;

        // Check if expired by date
        if (EndDate.HasValue && now > EndDate.Value)
        {
            Status = CouponStatus.Expired;
            return;
        }

        // Check if expired by usage
        if (MaxUsageCount > 0 && CurrentUsageCount >= MaxUsageCount)
        {
            Status = CouponStatus.Expired;
            return;
        }

        // Check if upcoming
        if (StartDate.HasValue && now < StartDate.Value)
        {
            Status = CouponStatus.UpcomingActive;
            return;
        }

        // Otherwise, it's active
        Status = CouponStatus.Active;
    }

    /// <summary>
    /// Checks if this coupon is applicable to a specific category
    /// </summary>
    public bool IsApplicableToCategory(string? category)
    {
        if (Scope != DiscountScope.Category)
            return Scope == DiscountScope.Global || Scope == DiscountScope.Cart;

        if (string.IsNullOrEmpty(ApplicableCategories) || string.IsNullOrEmpty(category))
            return false;

        var categories = ApplicableCategories.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().ToLower());

        return categories.Contains(category.Trim().ToLower());
    }

    /// <summary>
    /// Gets the applicable tier for a given purchase amount
    /// </summary>
    public DiscountTier? GetApplicableTier(double amount)
    {
        if (!IsTiered || !Tiers.Any())
            return null;

        return Tiers
            .Where(t => t.IsApplicable(amount))
            .OrderBy(t => t.Order)
            .FirstOrDefault();
    }

    /// <summary>
    /// Checks if this coupon is an automatic campaign discount
    /// </summary>
    public bool IsAutomaticCampaign()
    {
        return IsAutomatic && CampaignType != CampaignType.None;
    }
}

public enum CouponStatus
{
    /// <summary>
    /// Coupon is currently active and can be used
    /// </summary>
    Active = 0,

    /// <summary>
    /// Coupon has expired (past end date or max usage reached)
    /// </summary>
    Expired = 1,

    /// <summary>
    /// Coupon has been manually disabled
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// Coupon is scheduled to become active in the future
    /// </summary>
    UpcomingActive = 3
}