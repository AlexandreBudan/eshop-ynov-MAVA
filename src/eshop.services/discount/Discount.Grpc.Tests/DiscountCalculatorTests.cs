using Discount.Grpc.Models;
using Discount.Grpc.Services;
using FluentAssertions;
using Xunit;

namespace Discount.Grpc.Tests;

/// <summary>
/// Unit tests for the DiscountCalculator service covering various discount scenarios
/// </summary>
public class DiscountCalculatorTests
{
    private readonly DiscountCalculator _calculator;

    public DiscountCalculatorTests()
    {
        _calculator = new DiscountCalculator();
    }

    #region Single Coupon Tests

    [Fact]
    public void CalculateDiscount_WithSinglePercentageCoupon_ShouldApplyCorrectDiscount()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "10% off",
                Type = DiscountType.Percentage,
                PercentageDiscount = 10,
                Status = CouponStatus.Active,
                IsStackable = true
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(10.0);
        finalPrice.Should().Be(90.0);
        appliedDiscounts.Should().HaveCount(1);
        appliedDiscounts[0].Description.Should().Be("10% off");
        appliedDiscounts[0].DiscountAmount.Should().Be(10.0);
        warningMessage.Should().BeNull();
    }

    [Fact]
    public void CalculateDiscount_WithSingleFixedCoupon_ShouldApplyCorrectDiscount()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "20€ off",
                Type = DiscountType.Fixed,
                Amount = 20.0,
                Status = CouponStatus.Active,
                IsStackable = true
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(20.0);
        finalPrice.Should().Be(80.0);
        appliedDiscounts.Should().HaveCount(1);
        appliedDiscounts[0].DiscountAmount.Should().Be(20.0);
    }

    [Fact]
    public void CalculateDiscount_WithCombinedCoupon_ShouldApplyBothDiscounts()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "10% + 5€ off",
                Type = DiscountType.Combined,
                PercentageDiscount = 10,
                Amount = 5.0,
                Status = CouponStatus.Active,
                IsStackable = true
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(15.0); // 10% of 100 = 10€ + 5€ = 15€
        finalPrice.Should().Be(85.0);
        appliedDiscounts.Should().HaveCount(1);
    }

    #endregion

    #region Stacked Discounts Tests

    [Fact]
    public void CalculateDiscount_WithStackablePercentageCoupons_ShouldApplyInOrder()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "10% off - Priority 2",
                Type = DiscountType.Percentage,
                PercentageDiscount = 10,
                Status = CouponStatus.Active,
                IsStackable = true,
                Priority = 2
            },
            new Coupon
            {
                Id = 2,
                Description = "15% off - Priority 1",
                Type = DiscountType.Percentage,
                PercentageDiscount = 15,
                Status = CouponStatus.Active,
                IsStackable = true,
                Priority = 1
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        // Higher priority (2) applied first: 100 * 0.10 = 10, remaining 90
        // Then priority 1: 90 * 0.15 = 13.5, remaining 76.5
        totalDiscount.Should().Be(23.5);
        finalPrice.Should().Be(76.5);
        appliedDiscounts.Should().HaveCount(2);
        appliedDiscounts[0].Description.Should().Be("10% off - Priority 2");
        appliedDiscounts[1].Description.Should().Be("15% off - Priority 1");
    }

    [Fact]
    public void CalculateDiscount_WithPercentageAndFixedCoupons_ShouldApplyPercentageFirst()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "10€ off",
                Type = DiscountType.Fixed,
                Amount = 10.0,
                Status = CouponStatus.Active,
                IsStackable = true,
                Priority = 0
            },
            new Coupon
            {
                Id = 2,
                Description = "20% off",
                Type = DiscountType.Percentage,
                PercentageDiscount = 20,
                Status = CouponStatus.Active,
                IsStackable = true,
                Priority = 0
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        // Percentage applied first (same priority): 100 * 0.20 = 20, remaining 80
        // Then fixed: 80 - 10 = 70
        totalDiscount.Should().Be(30.0);
        finalPrice.Should().Be(70.0);
        appliedDiscounts.Should().HaveCount(2);
    }

    [Fact]
    public void CalculateDiscount_ExceedingMaxStackPercentage_ShouldCapAndWarn()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "20% off",
                Type = DiscountType.Percentage,
                PercentageDiscount = 20,
                Status = CouponStatus.Active,
                IsStackable = true,
                MaxStackPercentage = 30.0
            },
            new Coupon
            {
                Id = 2,
                Description = "15% off",
                Type = DiscountType.Percentage,
                PercentageDiscount = 15,
                Status = CouponStatus.Active,
                IsStackable = true,
                MaxStackPercentage = 30.0
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        // 20% applied first: 100 * 0.20 = 20, remaining 80
        // Only 10% can be applied from second coupon (to reach max 30%)
        // 80 * 0.10 = 8, total discount = 28
        warningMessage.Should().NotBeNull();
        warningMessage.Should().Contain("Maximum stacking limit");
        totalDiscount.Should().BeLessThanOrEqualTo(30.0);
    }

    #endregion

    #region Non-Stackable Coupon Tests

    [Fact]
    public void CalculateDiscount_WithNonStackableCoupon_ShouldApplyOnlyThatCoupon()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "10% off",
                Type = DiscountType.Percentage,
                PercentageDiscount = 10,
                Status = CouponStatus.Active,
                IsStackable = true,
                CouponCode = "SAVE10"
            },
            new Coupon
            {
                Id = 2,
                Description = "50% off - Non stackable",
                Type = DiscountType.Percentage,
                PercentageDiscount = 50,
                Status = CouponStatus.Active,
                IsStackable = false,
                CouponCode = "MEGA50",
                MaxStackPercentage = 100.0
            }
        };

        // Act - User provides both codes
        var requestedCodes = new List<string> { "SAVE10", "MEGA50" };
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons, requestedCodes);

        // Assert
        totalDiscount.Should().Be(50.0);
        finalPrice.Should().Be(50.0);
        appliedDiscounts.Should().HaveCount(1);
        appliedDiscounts[0].Description.Should().Be("50% off - Non stackable");
        warningMessage.Should().NotBeNull();
        warningMessage.Should().Contain("not stackable");
    }

    #endregion

    #region Invalid/Expired Coupon Tests

    [Fact]
    public void CalculateDiscount_WithExpiredCoupon_ShouldNotApplyDiscount()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "Expired coupon",
                Type = DiscountType.Percentage,
                PercentageDiscount = 20,
                Status = CouponStatus.Expired,
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(-1)
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(0);
        finalPrice.Should().Be(originalPrice);
        appliedDiscounts.Should().BeEmpty();
        warningMessage.Should().NotBeNull();
    }

    [Fact]
    public void CalculateDiscount_WithDisabledCoupon_ShouldNotApplyDiscount()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "Disabled coupon",
                Type = DiscountType.Percentage,
                PercentageDiscount = 20,
                Status = CouponStatus.Disabled
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(0);
        finalPrice.Should().Be(originalPrice);
        appliedDiscounts.Should().BeEmpty();
    }

    [Fact]
    public void CalculateDiscount_WithMaxUsageReached_ShouldNotApplyDiscount()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "Max usage reached",
                Type = DiscountType.Percentage,
                PercentageDiscount = 20,
                Status = CouponStatus.Active,
                MaxUsageCount = 10,
                CurrentUsageCount = 10
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(0);
        finalPrice.Should().Be(originalPrice);
        appliedDiscounts.Should().BeEmpty();
    }

    [Fact]
    public void CalculateDiscount_BelowMinimumPurchaseAmount_ShouldNotApplyDiscount()
    {
        var originalPrice = 50.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "20% off for orders over 100€",
                Type = DiscountType.Percentage,
                PercentageDiscount = 20,
                Status = CouponStatus.Active,
                MinimumPurchaseAmount = 100.0
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(0);
        finalPrice.Should().Be(originalPrice);
        appliedDiscounts.Should().BeEmpty();
    }

    #endregion

    #region Contextual Rules Tests

    [Fact]
    public void CalculateDiscount_WithCategoryScope_MatchingCategory_ShouldApplyDiscount()
    {
        var originalPrice = 100.0;
        var categories = new List<string> { "Electronics", "Computers" };
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "10% off Electronics",
                Type = DiscountType.Percentage,
                PercentageDiscount = 10,
                Status = CouponStatus.Active,
                Scope = DiscountScope.Category,
                ApplicableCategories = "Electronics,Phones"
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons, null, categories);

        // Assert
        totalDiscount.Should().Be(10.0);
        finalPrice.Should().Be(90.0);
        appliedDiscounts.Should().HaveCount(1);
    }

    [Fact]
    public void CalculateDiscount_WithCategoryScope_NonMatchingCategory_ShouldNotApplyDiscount()
    {
        var originalPrice = 100.0;
        var categories = new List<string> { "Books", "Music" };
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "10% off Electronics",
                Type = DiscountType.Percentage,
                PercentageDiscount = 10,
                Status = CouponStatus.Active,
                Scope = DiscountScope.Category,
                ApplicableCategories = "Electronics,Phones"
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons, null, categories);

        // Assert
        totalDiscount.Should().Be(0);
        finalPrice.Should().Be(originalPrice);
        appliedDiscounts.Should().BeEmpty();
    }

    [Fact]
    public void CalculateDiscount_WithGlobalScope_ShouldAlwaysApply()
    {
        var originalPrice = 100.0;
        var categories = new List<string> { "AnyCategory" };
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "Global 5% discount",
                Type = DiscountType.Percentage,
                PercentageDiscount = 5,
                Status = CouponStatus.Active,
                Scope = DiscountScope.Global
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons, null, categories);

        // Assert
        totalDiscount.Should().Be(5.0);
        finalPrice.Should().Be(95.0);
        appliedDiscounts.Should().HaveCount(1);
    }

    [Fact]
    public void CalculateDiscount_WithTieredDiscount_ShouldApplyCorrectTier()
    {
        var originalPrice = 250.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "Tiered discount",
                Type = DiscountType.Percentage,
                Status = CouponStatus.Active,
                IsTiered = true,
                Tiers = new List<DiscountTier>
                {
                    new DiscountTier { MinAmount = 0, MaxAmount = 100, PercentageDiscount = 5, Order = 1 },
                    new DiscountTier { MinAmount = 100, MaxAmount = 200, PercentageDiscount = 10, Order = 2 },
                    new DiscountTier { MinAmount = 200, MaxAmount = null, PercentageDiscount = 15, Order = 3 }
                }
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        // Should apply 15% tier (for amounts >= 200)
        totalDiscount.Should().Be(37.5); // 250 * 0.15 = 37.5
        finalPrice.Should().Be(212.5);
        appliedDiscounts.Should().HaveCount(1);
    }

    [Fact]
    public void CalculateDiscount_WithAutomaticCampaign_ShouldApplyWithoutCode()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "Black Friday automatic discount",
                Type = DiscountType.Percentage,
                PercentageDiscount = 20,
                Status = CouponStatus.Active,
                IsAutomatic = true,
                CampaignType = CampaignType.BlackFriday,
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1)
            }
        };

        // Act - No coupon codes provided
        var (totalDiscount, finalPrice, appliedDiscounts, _) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(20.0);
        finalPrice.Should().Be(80.0);
        appliedDiscounts.Should().HaveCount(1);
        appliedDiscounts[0].Description.Should().Be("Black Friday automatic discount");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CalculateDiscount_WithZeroPrice_ShouldReturnZeroDiscount()
    {
        var originalPrice = 0.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "10% off",
                Type = DiscountType.Percentage,
                PercentageDiscount = 10,
                Status = CouponStatus.Active
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(0);
        finalPrice.Should().Be(0);
        warningMessage.Should().NotBeNull();
    }

    [Fact]
    public void CalculateDiscount_WithNoCoupons_ShouldReturnOriginalPrice()
    {
        var originalPrice = 100.0;
        var coupons = new List<Coupon>();

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(0);
        finalPrice.Should().Be(originalPrice);
        appliedDiscounts.Should().BeEmpty();
    }

    [Fact]
    public void CalculateDiscount_DiscountExceedingPrice_ShouldCapAtZero()
    {
        var originalPrice = 50.0;
        var coupons = new List<Coupon>
        {
            new Coupon
            {
                Id = 1,
                Description = "100€ off",
                Type = DiscountType.Fixed,
                Amount = 100.0,
                Status = CouponStatus.Active
            }
        };

        // Act
        var (totalDiscount, finalPrice, appliedDiscounts, warningMessage) =
            _calculator.CalculateDiscount(originalPrice, coupons);

        // Assert
        totalDiscount.Should().Be(50.0);
        finalPrice.Should().Be(0);
        warningMessage.Should().NotBeNull();
        warningMessage.Should().Contain("capped at current price");
    }

    #endregion
}
