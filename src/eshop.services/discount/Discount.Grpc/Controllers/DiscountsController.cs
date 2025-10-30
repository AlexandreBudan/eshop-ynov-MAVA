using Discount.Grpc.Data;
using Discount.Grpc.DTOs;
using Discount.Grpc.Models;
using Discount.Grpc.Services;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Controllers;

/// <summary>
/// REST API controller for discount application and validation
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DiscountsController(
    DiscountContext dbContext,
    DiscountCalculator calculator,
    ILogger<DiscountsController> logger) : ControllerBase
{
    private readonly DiscountContext _dbContext = dbContext;
    private readonly DiscountCalculator _calculator = calculator;
    private readonly ILogger<DiscountsController> _logger = logger;

    /// <summary>
    /// Apply discounts to a product and calculate the final price
    /// </summary>
    /// <param name="request">Product and discount information</param>
    /// <returns>Calculated discount with final price</returns>
    /// <response code="200">Discount successfully applied</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Product not found or no discounts available</response>
    [HttpPost("apply")]
    [ProducesResponseType(typeof(ApplyDiscountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplyDiscountResponse>> ApplyDiscount([FromBody] ApplyDiscountRequest request)
    {
        _logger.LogInformation("Applying discount for Product: {ProductId}, Price: {Price}, Codes: {Codes}",
            request.ProductId, request.OriginalPrice, string.Join(",", request.CouponCodes ?? new List<string>()));

        if (request.OriginalPrice <= 0)
        {
            return BadRequest(new { error = "Original price must be greater than zero" });
        }

        var query = _dbContext.Coupons
            .Include(c => c.Tiers)
            .AsQueryable();

        query = query.Where(c => c.ProductName == request.ProductName || c.ProductId == request.ProductId);

        var coupons = await query.ToListAsync();

        if (coupons.Count == 0)
        {
            return NotFound(new { error = "No discounts found for this product" });
        }

        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(
                request.OriginalPrice,
                coupons,
                request.CouponCodes,
                request.Categories);

        var response = new ApplyDiscountResponse
        {
            TotalDiscount = totalDiscount,
            FinalPrice = finalPrice,
            OriginalPrice = request.OriginalPrice,
            DiscountPercentage = request.OriginalPrice > 0 ? (totalDiscount / request.OriginalPrice) * 100 : 0,
            WarningMessage = warningMessage,
            AppliedDiscounts = [.. appliedDiscounts.Select(d => new AppliedDiscountDetail
            {
                Description = d.Description,
                DiscountAmount = d.DiscountAmount,
                Type = d.Type.ToString()
            })]
        };

        _logger.LogInformation("Discount applied: Total={Total}, Final={Final}, Count={Count}",
            totalDiscount, finalPrice, appliedDiscounts.Count);

        return Ok(response);
    }

    /// <summary>
    /// Validate a coupon code
    /// </summary>
    /// <param name="code">Coupon code to validate</param>
    /// <param name="purchaseAmount">Optional purchase amount to check minimum threshold</param>
    /// <param name="categories">Optional comma-separated product categories</param>
    /// <returns>Validation result with coupon details if valid</returns>
    /// <response code="200">Coupon validation result</response>
    /// <response code="400">Invalid request data</response>
    [HttpGet("validate/{code}")]
    [ProducesResponseType(typeof(ValidateCouponResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ValidateCouponResponse>> ValidateCoupon(
        string code,
        [FromQuery] double? purchaseAmount = null,
        [FromQuery] string? categories = null)
    {
        _logger.LogInformation("Validating coupon code: {Code}", code);

        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { error = "Coupon code is required" });
        }

        var coupon = await _dbContext.Coupons
            .Include(c => c.Tiers)
            .FirstOrDefaultAsync(c => c.CouponCode == code);

        var response = new ValidateCouponResponse();
        var validationErrors = new List<string>();

        if (coupon == null)
        {
            response.IsValid = false;
            response.ValidationMessage = "Coupon code not found";
            validationErrors.Add("COUPON_NOT_FOUND");
            response.ValidationErrors = validationErrors;
            return Ok(response);
        }

        coupon.UpdateStatus();

        if (!coupon.IsValid())
        {
            response.IsValid = false;
            response.ValidationMessage = "Coupon is not valid";

            if (coupon.Status != CouponStatus.Active)
            {
                validationErrors.Add($"COUPON_STATUS_{coupon.Status.ToString().ToUpper()}");
            }

            if (coupon.EndDate.HasValue && DateTime.UtcNow > coupon.EndDate.Value)
            {
                validationErrors.Add("COUPON_EXPIRED");
            }

            if (coupon.StartDate.HasValue && DateTime.UtcNow < coupon.StartDate.Value)
            {
                validationErrors.Add("COUPON_NOT_STARTED");
            }

            if (coupon.MaxUsageCount > 0 && coupon.CurrentUsageCount >= coupon.MaxUsageCount)
            {
                validationErrors.Add("COUPON_MAX_USAGE_REACHED");
            }

            response.ValidationErrors = validationErrors;
            return Ok(response);
        }

        // Check minimum purchase amount if provided
        if (purchaseAmount.HasValue && purchaseAmount.Value < coupon.MinimumPurchaseAmount)
        {
            response.IsValid = false;
            response.ValidationMessage = $"Minimum purchase amount of {coupon.MinimumPurchaseAmount}€ required";
            validationErrors.Add("MINIMUM_PURCHASE_NOT_MET");
            response.ValidationErrors = validationErrors;
            response.Coupon = coupon.Adapt<CouponResponseDto>();
            return Ok(response);
        }

        // Check category scope if categories provided
        if (!string.IsNullOrEmpty(categories))
        {
            var categoryList = categories.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (coupon.Scope == DiscountScope.Category && categoryList.Any())
            {
                var isApplicable = categoryList.Any(cat => coupon.IsApplicableToCategory(cat));
                if (!isApplicable)
                {
                    response.IsValid = false;
                    response.ValidationMessage = "Coupon not applicable to specified categories";
                    validationErrors.Add("CATEGORY_NOT_APPLICABLE");
                    response.ValidationErrors = validationErrors;
                    response.Coupon = coupon.Adapt<CouponResponseDto>();
                    return Ok(response);
                }
            }
        }

        // Coupon is valid
        response.IsValid = true;
        response.ValidationMessage = "Coupon is valid";
        response.Coupon = coupon.Adapt<CouponResponseDto>();
        response.ValidationErrors = validationErrors;

        _logger.LogInformation("Coupon {Code} validated successfully", code);

        return Ok(response);
    }

    /// <summary>
    /// Get all available discounts for a specific product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="includeCodeBased">Include code-based discounts (default: true)</param>
    /// <returns>List of available discounts</returns>
    /// <response code="200">Discounts retrieved successfully</response>
    /// <response code="404">Product not found</response>
    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(ProductDiscountsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDiscountsResponse>> GetProductDiscounts(
        string productId,
        [FromQuery] bool includeCodeBased = true)
    {
        _logger.LogInformation("Getting discounts for product: {ProductId}", productId);

        var query = _dbContext.Coupons
            .Include(c => c.Tiers)
            .Where(c => c.ProductId == productId || c.ProductName == productId);

        var allCoupons = await query.ToListAsync();

        if (allCoupons.Count == 0)
        {
            return NotFound(new { error = "No discounts found for this product" });
        }

        // Update statuses and filter valid coupons
        allCoupons.ForEach(c => c.UpdateStatus());
        var validCoupons = allCoupons.Where(c => c.IsValid()).ToList();

        // Separate automatic and code-based discounts
        var automaticDiscounts = validCoupons
            .Where(c => string.IsNullOrEmpty(c.CouponCode) || c.IsAutomatic)
            .ToList();

        var codeBasedDiscounts = includeCodeBased
            ? validCoupons.Where(c => !string.IsNullOrEmpty(c.CouponCode) && !c.IsAutomatic).ToList()
            : [];

        var allDiscounts = automaticDiscounts.Concat(codeBasedDiscounts).ToList();

        var bestAutomatic = automaticDiscounts
            .OrderByDescending(c => c.Type == DiscountType.Percentage ? c.PercentageDiscount : c.Amount)
            .FirstOrDefault();

        var response = new ProductDiscountsResponse
        {
            ProductId = productId,
            ProductName = allCoupons.FirstOrDefault()?.ProductName ?? productId,
            AvailableDiscounts = allDiscounts.Select(c => c.Adapt<CouponResponseDto>()).ToList(),
            BestAutomaticDiscount = bestAutomatic?.Adapt<CouponResponseDto>(),
            AutomaticDiscountCount = automaticDiscounts.Count,
            CodeBasedDiscountCount = codeBasedDiscounts.Count
        };

        _logger.LogInformation("Found {Count} discounts for product {ProductId}", allDiscounts.Count, productId);

        return Ok(response);
    }

    /// <summary>
    /// Get discount by coupon code (without validation)
    /// </summary>
    /// <param name="code">Coupon code</param>
    /// <returns>Coupon details</returns>
    /// <response code="200">Coupon found</response>
    /// <response code="404">Coupon not found</response>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(CouponResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponResponseDto>> GetDiscountByCode(string code)
    {
        _logger.LogInformation("Getting discount by code: {Code}", code);

        var coupon = await _dbContext.Coupons
            .Include(c => c.Tiers)
            .FirstOrDefaultAsync(c => c.CouponCode == code);

        if (coupon == null)
        {
            return NotFound(new { error = "Coupon not found" });
        }

        coupon.UpdateStatus();

        return Ok(coupon.Adapt<CouponResponseDto>());
    }

    /// <summary>
    /// Get all active automatic campaigns
    /// </summary>
    /// <param name="campaignType">Optional filter by campaign type</param>
    /// <returns>List of active automatic campaigns</returns>
    /// <response code="200">Campaigns retrieved successfully</response>
    [HttpGet("campaigns")]
    [ProducesResponseType(typeof(PagedResultDto<CouponResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<CouponResponseDto>>> GetActiveCampaigns(
        [FromQuery] CampaignType? campaignType = null)
    {
        _logger.LogInformation("Getting active campaigns, type: {Type}", campaignType);

        var query = _dbContext.Coupons
            .Include(c => c.Tiers)
            .Where(c => c.IsAutomatic);

        if (campaignType.HasValue)
        {
            query = query.Where(c => c.CampaignType == campaignType.Value);
        }

        var campaigns = await query.ToListAsync();

        campaigns.ForEach(c => c.UpdateStatus());
        var validCampaigns = campaigns.Where(c => c.IsValid()).ToList();

        var response = new PagedResultDto<CouponResponseDto>
        {
            Items = validCampaigns.Select(c => c.Adapt<CouponResponseDto>()).ToList(),
            TotalCount = validCampaigns.Count,
            PageNumber = 1,
            PageSize = validCampaigns.Count
        };

        _logger.LogInformation("Found {Count} active campaigns", validCampaigns.Count);

        return Ok(response);
    }

    /// <summary>
    /// Calculate the best possible discount for a product with given coupon codes
    /// </summary>
    /// <param name="request">Product and coupon information</param>
    /// <returns>Best discount combination</returns>
    /// <response code="200">Best discount calculated</response>
    /// <response code="400">Invalid request</response>
    [HttpPost("calculate-best")]
    [ProducesResponseType(typeof(ApplyDiscountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApplyDiscountResponse>> CalculateBestDiscount([FromBody] ApplyDiscountRequest request)
    {
        _logger.LogInformation("Calculating best discount for Product: {ProductId}, Price: {Price}",
            request.ProductId, request.OriginalPrice);

        if (request.OriginalPrice <= 0)
        {
            return BadRequest(new { error = "Original price must be greater than zero" });
        }

        var allCoupons = await _dbContext.Coupons
            .Include(c => c.Tiers)
            .Where(c => c.ProductName == request.ProductName || c.ProductId == request.ProductId)
            .ToListAsync();

        if (allCoupons.Count == 0)
        {
            return Ok(new ApplyDiscountResponse
            {
                TotalDiscount = 0,
                FinalPrice = request.OriginalPrice,
                OriginalPrice = request.OriginalPrice,
                DiscountPercentage = 0,
                WarningMessage = "No discounts available"
            });
        }

        allCoupons.ForEach(c => c.UpdateStatus());

        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(
                request.OriginalPrice,
                allCoupons,
                request.CouponCodes,
                request.Categories);

        var response = new ApplyDiscountResponse
        {
            TotalDiscount = totalDiscount,
            FinalPrice = finalPrice,
            OriginalPrice = request.OriginalPrice,
            DiscountPercentage = request.OriginalPrice > 0 ? totalDiscount / request.OriginalPrice * 100 : 0,
            WarningMessage = warningMessage,
            AppliedDiscounts = [.. appliedDiscounts.Select(d => new AppliedDiscountDetail
            {
                Description = d.Description,
                DiscountAmount = d.DiscountAmount,
                Type = d.Type.ToString()
            })]
        };

        return Ok(response);
    }
}
