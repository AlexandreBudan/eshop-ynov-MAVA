using Discount.Grpc.Models;

namespace Discount.Grpc.DTOs;

/// <summary>
/// DTO for creating a new coupon
/// </summary>
public class CreateCouponDto
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Amount { get; set; }
    public DiscountType Type { get; set; }
    public double PercentageDiscount { get; set; }
    public string? CouponCode { get; set; }
    public bool IsStackable { get; set; } = true;
    public double MaxStackPercentage { get; set; } = 30.0;
    public int Priority { get; set; } = 0;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double MinimumPurchaseAmount { get; set; } = 0;
    public int MaxUsageCount { get; set; } = 0;
    public DiscountScope Scope { get; set; } = DiscountScope.Product;
    public string? ApplicableCategories { get; set; }
    public CampaignType CampaignType { get; set; } = CampaignType.None;
    public bool IsAutomatic { get; set; } = false;
}

/// <summary>
/// DTO for updating an existing coupon
/// </summary>
public class UpdateCouponDto
{
    public string? ProductName { get; set; }
    public string? ProductId { get; set; }
    public string? Description { get; set; }
    public double? Amount { get; set; }
    public DiscountType? Type { get; set; }
    public double? PercentageDiscount { get; set; }
    public string? CouponCode { get; set; }
    public bool? IsStackable { get; set; }
    public double? MaxStackPercentage { get; set; }
    public int? Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double? MinimumPurchaseAmount { get; set; }
    public int? MaxUsageCount { get; set; }
    public CouponStatus? Status { get; set; }
    public DiscountScope? Scope { get; set; }
    public string? ApplicableCategories { get; set; }
    public CampaignType? CampaignType { get; set; }
    public bool? IsAutomatic { get; set; }
}

/// <summary>
/// DTO for coupon response
/// </summary>
public class CouponResponseDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Amount { get; set; }
    public DiscountType Type { get; set; }
    public double PercentageDiscount { get; set; }
    public string? CouponCode { get; set; }
    public bool IsStackable { get; set; }
    public double MaxStackPercentage { get; set; }
    public int Priority { get; set; }
    public CouponStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double MinimumPurchaseAmount { get; set; }
    public int MaxUsageCount { get; set; }
    public int CurrentUsageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsCurrentlyValid { get; set; }
}

/// <summary>
/// DTO for coupon list filters
/// </summary>
public class CouponFilterDto
{
    public CouponStatus? Status { get; set; }
    public string? ProductName { get; set; }
    public string? ProductId { get; set; }
    public string? CouponCode { get; set; }
    public DiscountType? Type { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
}

/// <summary>
/// DTO for paginated results
/// </summary>
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
