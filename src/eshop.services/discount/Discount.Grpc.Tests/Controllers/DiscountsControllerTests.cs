using Discount.Grpc.Controllers;
using Discount.Grpc.Data;
using Discount.Grpc.DTOs;
using Discount.Grpc.Models;
using Discount.Grpc.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Discount.Grpc.Tests.Controllers;

/// <summary>
/// Unit tests for DiscountsController covering discount application, validation, and retrieval
/// </summary>
public class DiscountsControllerTests : IDisposable
{
    private readonly DiscountContext _context;
    private readonly DiscountsController _controller;
    private readonly DiscountCalculator _calculator;
    private readonly Mock<ILogger<DiscountsController>> _loggerMock;

    public DiscountsControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DiscountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DiscountContext(options);
        _calculator = new DiscountCalculator();
        _loggerMock = new Mock<ILogger<DiscountsController>>();
        _controller = new DiscountsController(_context, _calculator, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region ApplyDiscount Tests

    [Fact]
    public async Task ApplyDiscount_WithValidProduct_ShouldReturnDiscountedPrice()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "iPhone 15",
            ProductId = "IPHONE-15",
            Description = "10% off iPhone",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        var request = new ApplyDiscountRequest
        {
            ProductId = "IPHONE-15",
            ProductName = "iPhone 15",
            OriginalPrice = 1000.0
        };

        // Act
        var result = await _controller.ApplyDiscount(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApplyDiscountResponse>().Subject;

        response.OriginalPrice.Should().Be(1000.0);
        response.TotalDiscount.Should().Be(100.0);
        response.FinalPrice.Should().Be(900.0);
        response.DiscountPercentage.Should().Be(10.0);
        response.AppliedDiscounts.Should().HaveCount(1);
    }

    [Fact]
    public async Task ApplyDiscount_WithZeroPrice_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ApplyDiscountRequest
        {
            ProductId = "TEST-001",
            ProductName = "Test Product",
            OriginalPrice = 0
        };

        // Act
        var result = await _controller.ApplyDiscount(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ApplyDiscount_WithNegativePrice_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ApplyDiscountRequest
        {
            ProductId = "TEST-001",
            ProductName = "Test Product",
            OriginalPrice = -100
        };

        // Act
        var result = await _controller.ApplyDiscount(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ApplyDiscount_WithNoAvailableCoupons_ShouldReturnNotFound()
    {
        // Arrange
        var request = new ApplyDiscountRequest
        {
            ProductId = "NONEXISTENT",
            ProductName = "Nonexistent Product",
            OriginalPrice = 100.0
        };

        // Act
        var result = await _controller.ApplyDiscount(request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ApplyDiscount_WithCouponCode_ShouldApplyCodeBasedDiscount()
    {
        // Arrange
        var automaticCoupon = new Coupon
        {
            ProductName = "iPhone 15",
            ProductId = "IPHONE-15",
            Description = "5% automatic discount",
            Type = DiscountType.Percentage,
            PercentageDiscount = 5,
            Status = CouponStatus.Active,
            IsAutomatic = true,
            IsStackable = true
        };
        var codeCoupon = new Coupon
        {
            ProductName = "iPhone 15",
            ProductId = "IPHONE-15",
            Description = "Extra 10% with code",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active,
            CouponCode = "SAVE10",
            IsStackable = true
        };
        _context.Coupons.AddRange(automaticCoupon, codeCoupon);
        await _context.SaveChangesAsync();

        var request = new ApplyDiscountRequest
        {
            ProductId = "IPHONE-15",
            ProductName = "iPhone 15",
            OriginalPrice = 1000.0,
            CouponCodes = new List<string> { "SAVE10" }
        };

        // Act
        var result = await _controller.ApplyDiscount(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApplyDiscountResponse>().Subject;

        response.AppliedDiscounts.Should().HaveCount(2);
        response.TotalDiscount.Should().BeGreaterThan(100.0); // More than just one discount
    }

    #endregion

    #region ValidateCoupon Tests

    [Fact]
    public async Task ValidateCoupon_WithValidCode_ShouldReturnValid()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            CouponCode = "VALID10",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.ValidateCoupon("VALID10");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ValidateCouponResponse>().Subject;

        response.IsValid.Should().BeTrue();
        response.ValidationMessage.Should().Contain("valid");
        response.Coupon.Should().NotBeNull();
        response.Coupon!.CouponCode.Should().Be("VALID10");
    }

    [Fact]
    public async Task ValidateCoupon_WithNonExistentCode_ShouldReturnInvalid()
    {
        // Act
        var result = await _controller.ValidateCoupon("NONEXISTENT");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ValidateCouponResponse>().Subject;

        response.IsValid.Should().BeFalse();
        response.ValidationMessage.Should().Contain("not found");
        response.ValidationErrors.Should().Contain("COUPON_NOT_FOUND");
    }

    [Fact]
    public async Task ValidateCoupon_WithExpiredCode_ShouldReturnInvalid()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            CouponCode = "EXPIRED10",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Expired,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-1)
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.ValidateCoupon("EXPIRED10");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ValidateCouponResponse>().Subject;

        response.IsValid.Should().BeFalse();
        response.ValidationErrors.Should().Contain("COUPON_EXPIRED");
    }

    [Fact]
    public async Task ValidateCoupon_BelowMinimumPurchase_ShouldReturnInvalid()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            CouponCode = "MIN100",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active,
            MinimumPurchaseAmount = 100.0,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.ValidateCoupon("MIN100", purchaseAmount: 50.0);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ValidateCouponResponse>().Subject;

        response.IsValid.Should().BeFalse();
        response.ValidationMessage.Should().Contain("Minimum purchase");
        response.ValidationErrors.Should().Contain("MINIMUM_PURCHASE_NOT_MET");
    }

    [Fact]
    public async Task ValidateCoupon_WithEmptyCode_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.ValidateCoupon("");

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetProductDiscounts Tests

    [Fact]
    public async Task GetProductDiscounts_WithExistingProduct_ShouldReturnDiscounts()
    {
        // Arrange
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                ProductName = "iPhone 15",
                ProductId = "IPHONE-15",
                Description = "10% automatic",
                Type = DiscountType.Percentage,
                PercentageDiscount = 10,
                Status = CouponStatus.Active,
                IsAutomatic = true
            },
            new Coupon
            {
                ProductName = "iPhone 15",
                ProductId = "IPHONE-15",
                Description = "15% with code",
                Type = DiscountType.Percentage,
                PercentageDiscount = 15,
                Status = CouponStatus.Active,
                CouponCode = "SAVE15"
            }
        };
        _context.Coupons.AddRange(coupons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProductDiscounts("IPHONE-15");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ProductDiscountsResponse>().Subject;

        response.ProductId.Should().Be("IPHONE-15");
        response.AvailableDiscounts.Should().HaveCount(2);
        response.AutomaticDiscountCount.Should().Be(1);
        response.CodeBasedDiscountCount.Should().Be(1);
        response.BestAutomaticDiscount.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductDiscounts_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetProductDiscounts("NONEXISTENT");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetProductDiscounts_ExcludingCodeBased_ShouldReturnOnlyAutomatic()
    {
        // Arrange
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                ProductName = "iPhone 15",
                ProductId = "IPHONE-15",
                Description = "10% automatic",
                Type = DiscountType.Percentage,
                PercentageDiscount = 10,
                Status = CouponStatus.Active,
                IsAutomatic = true
            },
            new Coupon
            {
                ProductName = "iPhone 15",
                ProductId = "IPHONE-15",
                Description = "15% with code",
                Type = DiscountType.Percentage,
                PercentageDiscount = 15,
                Status = CouponStatus.Active,
                CouponCode = "SAVE15"
            }
        };
        _context.Coupons.AddRange(coupons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProductDiscounts("IPHONE-15", includeCodeBased: false);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ProductDiscountsResponse>().Subject;

        response.AvailableDiscounts.Should().HaveCount(1);
        response.AutomaticDiscountCount.Should().Be(1);
        response.CodeBasedDiscountCount.Should().Be(0);
    }

    #endregion

    #region GetDiscountByCode Tests

    [Fact]
    public async Task GetDiscountByCode_WithExistingCode_ShouldReturnCoupon()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            CouponCode = "TESTCODE",
            Type = DiscountType.Percentage,
            PercentageDiscount = 20,
            Status = CouponStatus.Active
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetDiscountByCode("TESTCODE");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CouponResponseDto>().Subject;

        response.CouponCode.Should().Be("TESTCODE");
        response.PercentageDiscount.Should().Be(20);
    }

    [Fact]
    public async Task GetDiscountByCode_WithNonExistentCode_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetDiscountByCode("NONEXISTENT");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetActiveCampaigns Tests

    [Fact]
    public async Task GetActiveCampaigns_WithAutomaticCoupons_ShouldReturnCampaigns()
    {
        // Arrange
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                ProductName = "Product 1",
                ProductId = "P1",
                Description = "Black Friday",
                Type = DiscountType.Percentage,
                PercentageDiscount = 30,
                Status = CouponStatus.Active,
                IsAutomatic = true,
                CampaignType = CampaignType.BlackFriday,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(7)
            },
            new Coupon
            {
                ProductName = "Product 2",
                ProductId = "P2",
                Description = "Flash Sale",
                Type = DiscountType.Percentage,
                PercentageDiscount = 25,
                Status = CouponStatus.Active,
                IsAutomatic = true,
                CampaignType = CampaignType.FlashSale,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(2)
            }
        };
        _context.Coupons.AddRange(coupons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetActiveCampaigns();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResultDto<CouponResponseDto>>().Subject;

        response.TotalCount.Should().Be(2);
        response.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveCampaigns_FilteredByCampaignType_ShouldReturnMatchingCampaigns()
    {
        // Arrange
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                ProductName = "Product 1",
                ProductId = "P1",
                Description = "Black Friday",
                Type = DiscountType.Percentage,
                PercentageDiscount = 30,
                Status = CouponStatus.Active,
                IsAutomatic = true,
                CampaignType = CampaignType.BlackFriday,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(7)
            },
            new Coupon
            {
                ProductName = "Product 2",
                ProductId = "P2",
                Description = "Seasonal",
                Type = DiscountType.Percentage,
                PercentageDiscount = 20,
                Status = CouponStatus.Active,
                IsAutomatic = true,
                CampaignType = CampaignType.SeasonalSales,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(30)
            }
        };
        _context.Coupons.AddRange(coupons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetActiveCampaigns(campaignType: CampaignType.BlackFriday);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResultDto<CouponResponseDto>>().Subject;

        response.TotalCount.Should().Be(1);
        response.Items.Should().HaveCount(1);
        response.Items.First().Description.Should().Contain("Black Friday");
    }

    #endregion

    #region CalculateBestDiscount Tests

    [Fact]
    public async Task CalculateBestDiscount_WithMultipleCoupons_ShouldReturnBestCombination()
    {
        // Arrange
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                ProductName = "Laptop",
                ProductId = "LAPTOP-001",
                Description = "10% discount",
                Type = DiscountType.Percentage,
                PercentageDiscount = 10,
                Status = CouponStatus.Active,
                IsStackable = true
            },
            new Coupon
            {
                ProductName = "Laptop",
                ProductId = "LAPTOP-001",
                Description = "50€ off",
                Type = DiscountType.Fixed,
                Amount = 50,
                Status = CouponStatus.Active,
                IsStackable = true
            }
        };
        _context.Coupons.AddRange(coupons);
        await _context.SaveChangesAsync();

        var request = new ApplyDiscountRequest
        {
            ProductId = "LAPTOP-001",
            ProductName = "Laptop",
            OriginalPrice = 1000.0
        };

        // Act
        var result = await _controller.CalculateBestDiscount(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApplyDiscountResponse>().Subject;

        response.TotalDiscount.Should().BeGreaterThan(0);
        response.FinalPrice.Should().BeLessThan(1000.0);
        response.AppliedDiscounts.Should().HaveCount(2);
    }

    [Fact]
    public async Task CalculateBestDiscount_WithNoDiscounts_ShouldReturnOriginalPrice()
    {
        // Arrange
        var request = new ApplyDiscountRequest
        {
            ProductId = "NONEXISTENT",
            ProductName = "Nonexistent",
            OriginalPrice = 500.0
        };

        // Act
        var result = await _controller.CalculateBestDiscount(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApplyDiscountResponse>().Subject;

        response.TotalDiscount.Should().Be(0);
        response.FinalPrice.Should().Be(500.0);
        response.WarningMessage.Should().Contain("No discounts available");
    }

    #endregion
}
