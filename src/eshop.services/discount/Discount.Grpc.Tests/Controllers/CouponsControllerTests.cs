using Discount.Grpc.Controllers;
using Discount.Grpc.Data;
using Discount.Grpc.DTOs;
using Discount.Grpc.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Discount.Grpc.Tests.Controllers;

/// <summary>
/// Unit tests for CouponsController covering CRUD operations and coupon lifecycle management
/// </summary>
public class CouponsControllerTests : IDisposable
{
    private readonly DiscountContext _context;
    private readonly CouponsController _controller;
    private readonly Mock<ILogger<CouponsController>> _loggerMock;

    public CouponsControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DiscountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DiscountContext(options);
        _loggerMock = new Mock<ILogger<CouponsController>>();
        _controller = new CouponsController(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region CreateCoupon Tests

    [Fact]
    public async Task CreateCoupon_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var createDto = new CreateCouponDto
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            Description = "10% discount",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = await _controller.CreateCoupon(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<CouponResponseDto>().Subject;

        response.ProductName.Should().Be("Test Product");
        response.Type.Should().Be(DiscountType.Percentage);
        response.PercentageDiscount.Should().Be(10);
        response.Status.Should().Be(CouponStatus.Active);

        _context.Coupons.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateCoupon_WithExistingCouponCode_ShouldReturnBadRequest()
    {
        // Arrange
        var existingCoupon = new Coupon
        {
            ProductName = "Existing",
            ProductId = "EXIST-001",
            CouponCode = "SAVE10",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active
        };
        _context.Coupons.Add(existingCoupon);
        await _context.SaveChangesAsync();

        var createDto = new CreateCouponDto
        {
            ProductName = "New Product",
            ProductId = "NEW-001",
            CouponCode = "SAVE10", // Duplicate
            Type = DiscountType.Percentage,
            PercentageDiscount = 15
        };

        // Act
        var result = await _controller.CreateCoupon(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _context.Coupons.Should().HaveCount(1); // Should not create duplicate
    }

    [Fact]
    public async Task CreateCoupon_WithInvalidDates_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateCouponDto
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow // End before start
        };

        // Act
        var result = await _controller.CreateCoupon(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetCoupon Tests

    [Fact]
    public async Task GetCoupon_WithExistingId_ShouldReturnCoupon()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCoupon(coupon.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CouponResponseDto>().Subject;
        response.Id.Should().Be(coupon.Id);
        response.ProductName.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetCoupon_WithNonExistingId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetCoupon(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetCoupons Tests

    [Fact]
    public async Task GetCoupons_WithNoFilters_ShouldReturnAllCoupons()
    {
        // Arrange
        var coupons = new List<Coupon>
        {
            new Coupon { ProductName = "Product 1", ProductId = "P1", Type = DiscountType.Percentage, PercentageDiscount = 10, Status = CouponStatus.Active },
            new Coupon { ProductName = "Product 2", ProductId = "P2", Type = DiscountType.Fixed, Amount = 20, Status = CouponStatus.Active },
            new Coupon { ProductName = "Product 3", ProductId = "P3", Type = DiscountType.Percentage, PercentageDiscount = 15, Status = CouponStatus.Expired }
        };
        _context.Coupons.AddRange(coupons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCoupons(new CouponFilterDto(), pageNumber: 1, pageSize: 10);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResultDto<CouponResponseDto>>().Subject;

        response.TotalCount.Should().Be(3);
        response.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetCoupons_WithStatusFilter_ShouldReturnFilteredCoupons()
    {
        // Arrange
        var coupons = new List<Coupon>
        {
            new Coupon { ProductName = "Product 1", ProductId = "P1", Type = DiscountType.Percentage, PercentageDiscount = 10, Status = CouponStatus.Active },
            new Coupon { ProductName = "Product 2", ProductId = "P2", Type = DiscountType.Fixed, Amount = 20, Status = CouponStatus.Disabled },
            new Coupon { ProductName = "Product 3", ProductId = "P3", Type = DiscountType.Percentage, PercentageDiscount = 15, Status = CouponStatus.Active }
        };
        _context.Coupons.AddRange(coupons);
        await _context.SaveChangesAsync();

        var filter = new CouponFilterDto { Status = CouponStatus.Active };

        // Act
        var result = await _controller.GetCoupons(filter, pageNumber: 1, pageSize: 10);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResultDto<CouponResponseDto>>().Subject;

        response.TotalCount.Should().Be(2);
        response.Items.Should().OnlyContain(c => c.Status == CouponStatus.Active);
    }

    [Fact]
    public async Task GetCoupons_WithProductNameFilter_ShouldReturnMatchingCoupons()
    {
        // Arrange
        var coupons = new List<Coupon>
        {
            new Coupon { ProductName = "iPhone 15", ProductId = "P1", Type = DiscountType.Percentage, PercentageDiscount = 10, Status = CouponStatus.Active },
            new Coupon { ProductName = "Samsung Galaxy", ProductId = "P2", Type = DiscountType.Fixed, Amount = 20, Status = CouponStatus.Active },
            new Coupon { ProductName = "iPhone 14", ProductId = "P3", Type = DiscountType.Percentage, PercentageDiscount = 15, Status = CouponStatus.Active }
        };
        _context.Coupons.AddRange(coupons);
        await _context.SaveChangesAsync();

        var filter = new CouponFilterDto { ProductName = "iPhone" };

        // Act
        var result = await _controller.GetCoupons(filter, pageNumber: 1, pageSize: 10);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResultDto<CouponResponseDto>>().Subject;

        response.TotalCount.Should().Be(2);
        response.Items.Should().OnlyContain(c => c.ProductName.Contains("iPhone"));
    }

    [Fact]
    public async Task GetCoupons_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            _context.Coupons.Add(new Coupon
            {
                ProductName = $"Product {i}",
                ProductId = $"P{i}",
                Type = DiscountType.Percentage,
                PercentageDiscount = 10,
                Status = CouponStatus.Active
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCoupons(new CouponFilterDto(), pageNumber: 2, pageSize: 5);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResultDto<CouponResponseDto>>().Subject;

        response.TotalCount.Should().Be(15);
        response.Items.Should().HaveCount(5);
        response.PageNumber.Should().Be(2);
        response.PageSize.Should().Be(5);
    }

    #endregion

    #region UpdateCoupon Tests

    [Fact]
    public async Task UpdateCoupon_WithValidData_ShouldUpdateCoupon()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "Original Product",
            ProductId = "ORIG-001",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCouponDto
        {
            ProductName = "Updated Product",
            PercentageDiscount = 15,
            Description = "Updated description"
        };

        // Act
        var result = await _controller.UpdateCoupon(coupon.Id, updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CouponResponseDto>().Subject;

        response.ProductName.Should().Be("Updated Product");
        response.PercentageDiscount.Should().Be(15);
        response.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdateCoupon_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var updateDto = new UpdateCouponDto
        {
            ProductName = "Updated Product"
        };

        // Act
        var result = await _controller.UpdateCoupon(999, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateCoupon_WithDuplicateCouponCode_ShouldReturnBadRequest()
    {
        // Arrange
        var coupon1 = new Coupon
        {
            ProductName = "Product 1",
            ProductId = "P1",
            CouponCode = "SAVE10",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active
        };
        var coupon2 = new Coupon
        {
            ProductName = "Product 2",
            ProductId = "P2",
            CouponCode = "SAVE20",
            Type = DiscountType.Percentage,
            PercentageDiscount = 20,
            Status = CouponStatus.Active
        };
        _context.Coupons.AddRange(coupon1, coupon2);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateCouponDto
        {
            CouponCode = "SAVE10" // Try to use coupon1's code for coupon2
        };

        // Act
        var result = await _controller.UpdateCoupon(coupon2.Id, updateDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeleteCoupon Tests

    [Fact]
    public async Task DeleteCoupon_WithExistingId_ShouldDeleteCoupon()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteCoupon(coupon.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _context.Coupons.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteCoupon_WithNonExistingId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.DeleteCoupon(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DisableCoupon/EnableCoupon Tests

    [Fact]
    public async Task DisableCoupon_WithExistingId_ShouldDisableCoupon()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DisableCoupon(coupon.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CouponResponseDto>().Subject;
        response.Status.Should().Be(CouponStatus.Disabled);
    }

    [Fact]
    public async Task EnableCoupon_WithExistingId_ShouldRecalculateStatus()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Disabled,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.EnableCoupon(coupon.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CouponResponseDto>().Subject;
        // EnableCoupon now properly re-enables the coupon and recalculates status
        // Since dates are valid, it should become Active
        response.Status.Should().Be(CouponStatus.Active);
    }

    [Fact]
    public async Task EnableCoupon_WithExpiredDates_ShouldSetToExpired()
    {
        // Arrange
        var coupon = new Coupon
        {
            ProductName = "Test Product",
            ProductId = "TEST-001",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Disabled,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-1) // Already expired
        };
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.EnableCoupon(coupon.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CouponResponseDto>().Subject;
        // Even though we tried to enable it, UpdateStatus should detect it's expired
        response.Status.Should().Be(CouponStatus.Expired);
    }

    [Fact]
    public async Task DisableCoupon_WithNonExistingId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.DisableCoupon(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task EnableCoupon_WithNonExistingId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.EnableCoupon(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
