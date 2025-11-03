namespace Discount.Grpc.DTOs;

/// <summary>
/// Request to apply discounts to a product
/// </summary>
public class ApplyDiscountRequest
{
    /// <summary>
    /// Product identifier
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Product name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Original price before discounts
    /// </summary>
    public double OriginalPrice { get; set; }

    /// <summary>
    /// Optional list of coupon codes to apply
    /// </summary>
    public List<string>? CouponCodes { get; set; }

    /// <summary>
    /// Optional list of product categories for category-based discounts
    /// </summary>
    public List<string>? Categories { get; set; }
}

/// <summary>
/// Response with calculated discount details
/// </summary>
public class ApplyDiscountResponse
{
    /// <summary>
    /// Total discount amount
    /// </summary>
    public double TotalDiscount { get; set; }

    /// <summary>
    /// Final price after all discounts
    /// </summary>
    public double FinalPrice { get; set; }

    /// <summary>
    /// Original price before discounts
    /// </summary>
    public double OriginalPrice { get; set; }

    /// <summary>
    /// Percentage of total discount
    /// </summary>
    public double DiscountPercentage { get; set; }

    /// <summary>
    /// List of applied discounts with details
    /// </summary>
    public List<AppliedDiscountDetail> AppliedDiscounts { get; set; } = new();

    /// <summary>
    /// Warning message if any (e.g., max stacking limit reached)
    /// </summary>
    public string? WarningMessage { get; set; }
}

/// <summary>
/// Details about a single applied discount
/// </summary>
public class AppliedDiscountDetail
{
    /// <summary>
    /// Discount description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Discount amount
    /// </summary>
    public double DiscountAmount { get; set; }

    /// <summary>
    /// Discount type (Fixed, Percentage, Combined)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Coupon code if applicable
    /// </summary>
    public string? CouponCode { get; set; }
}

/// <summary>
/// Request to validate a coupon code
/// </summary>
public class ValidateCouponRequest
{
    /// <summary>
    /// Coupon code to validate
    /// </summary>
    public string CouponCode { get; set; } = string.Empty;

    /// <summary>
    /// Optional purchase amount to check minimum threshold
    /// </summary>
    public double? PurchaseAmount { get; set; }

    /// <summary>
    /// Optional product categories to check category scope
    /// </summary>
    public List<string>? Categories { get; set; }
}

/// <summary>
/// Response with coupon validation details
/// </summary>
public class ValidateCouponResponse
{
    /// <summary>
    /// Whether the coupon is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Coupon details if valid
    /// </summary>
    public CouponResponseDto? Coupon { get; set; }

    /// <summary>
    /// Validation error message if invalid
    /// </summary>
    public string? ValidationMessage { get; set; }

    /// <summary>
    /// Specific validation errors
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new();
}

/// <summary>
/// Response with product discount information
/// </summary>
public class ProductDiscountsResponse
{
    /// <summary>
    /// Product identifier
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Product name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// List of available discounts for this product
    /// </summary>
    public List<CouponResponseDto> AvailableDiscounts { get; set; } = new();

    /// <summary>
    /// Best automatic discount available (no code required)
    /// </summary>
    public CouponResponseDto? BestAutomaticDiscount { get; set; }

    /// <summary>
    /// Count of automatic discounts
    /// </summary>
    public int AutomaticDiscountCount { get; set; }

    /// <summary>
    /// Count of code-based discounts
    /// </summary>
    public int CodeBasedDiscountCount { get; set; }
}
