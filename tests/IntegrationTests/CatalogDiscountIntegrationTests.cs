using Catalog.API;
using Catalog.API.Features.Products.Queries.GetProducts;
using Catalog.API.Models;
using Discount.Grpc;
using Discount.Grpc.Data;
using Discount.Grpc.Models;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace IntegrationTests;

/// <summary>
/// Integration tests for Catalog API -> Discount gRPC service communication
/// Tests the real flow of retrieving products with discount application
/// </summary>
public class CatalogDiscountIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public CatalogDiscountIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_WithValidDiscount_ShouldApplyDiscount()
    {
        // Arrange: Setup discount in Discount service
        using var discountScope = _factory.DiscountServices.CreateScope();
        var discountContext = discountScope.ServiceProvider.GetRequiredService<DiscountContext>();

        var coupon = new Coupon
        {
            ProductName = "Laptop X",
            ProductId = "1",
            Description = "10% off Laptop X",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        discountContext.Coupons.Add(coupon);
        await discountContext.SaveChangesAsync();

        // Arrange: Setup catalog query
        using var catalogScope = _factory.CatalogServices.CreateScope();
        var handler = catalogScope.ServiceProvider.GetRequiredService<GetProductsQueryHandler>();

        // Act
        var result = await handler.Handle(new GetProductsQuery(1, 10, "Laptop X", null, null, null), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().NotBeEmpty();
        var product = result.Products.FirstOrDefault(p => p.Name == "Laptop X");
        product.Should().NotBeNull();
        product!.Price.Should().Be(900); // Assuming original price is 1000
        product.Discount.Should().NotBeNull();
        product.Discount!.HasDiscount.Should().BeTrue();
        product.Discount.Amount.Should().Be(100);
        product.Discount.FinalPrice.Should().Be(900);
    }

    [Fact]
    public async Task GetProducts_WithNoDiscount_ShouldKeepOriginalPrice()
    {
        // Arrange: No discount setup for "Monitor Y"
        using var catalogScope = _factory.CatalogServices.CreateScope();
        var handler = catalogScope.ServiceProvider.GetRequiredService<GetProductsQueryHandler>();

        // Act
        var result = await handler.Handle(new GetProductsQuery(1, 10, "Monitor Y", null, null, null), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().NotBeEmpty();
        var product = result.Products.FirstOrDefault(p => p.Name == "Monitor Y");
        product.Should().NotBeNull();
        product!.Price.Should().Be(300); // Assuming original price is 300
        product.Discount.Should().NotBeNull();
        product.Discount!.HasDiscount.Should().BeFalse();
        product.Discount.Amount.Should().Be(0);
        product.Discount.FinalPrice.Should().Be(300);
    }

    [Fact]
    public async Task GetProducts_WithExpiredDiscount_ShouldNotApplyDiscount()
    {
        // Arrange: Setup expired discount
        using var discountScope = _factory.DiscountServices.CreateScope();
        var discountContext = discountScope.ServiceProvider.GetRequiredService<DiscountContext>();

        var expiredCoupon = new Coupon
        {
            ProductName = "Old Phone",
            ProductId = "3",
            Description = "Expired 20% off",
            Type = DiscountType.Percentage,
            PercentageDiscount = 20,
            Status = CouponStatus.Expired,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-1)
        };
        discountContext.Coupons.Add(expiredCoupon);
        await discountContext.SaveChangesAsync();

        // Arrange: Setup catalog query
        using var catalogScope = _factory.CatalogServices.CreateScope();
        var handler = catalogScope.ServiceProvider.GetRequiredService<GetProductsQueryHandler>();

        // Act
        var result = await handler.Handle(new GetProductsQuery(1, 10, "Old Phone", null, null, null), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().NotBeEmpty();
        var product = result.Products.FirstOrDefault(p => p.Name == "Old Phone");
        product.Should().NotBeNull();
        product!.Price.Should().Be(200); // Assuming original price is 200
        product.Discount.Should().NotBeNull();
        product.Discount!.HasDiscount.Should().BeFalse();
        product.Discount.Amount.Should().Be(0);
        product.Discount.FinalPrice.Should().Be(200);
    }
}
