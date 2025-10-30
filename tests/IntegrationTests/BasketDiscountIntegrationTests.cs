using Basket.API;
using Basket.API.Data.Repositories;
using Basket.API.Features.Baskets.Commands.AddItemToBasket;
using Basket.API.Models;
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
/// Integration tests for Basket API → Discount gRPC service communication
/// Tests the real flow of adding items to basket with discount application
/// </summary>
public class BasketDiscountIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    public BasketDiscountIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task AddItemToBasket_WithValidDiscount_ShouldApplyDiscount()
    {
        // Arrange: Setup discount in Discount service
        using var discountScope = _factory.DiscountServices.CreateScope();
        var discountContext = discountScope.ServiceProvider.GetRequiredService<DiscountContext>();

        var coupon = new Coupon
        {
            ProductName = "iPhone 15 Pro",
            ProductId = "1",
            Description = "10% off iPhone",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        discountContext.Coupons.Add(coupon);
        await discountContext.SaveChangesAsync();

        // Arrange: Setup basket command
        using var basketScope = _factory.BasketServices.CreateScope();
        var basketRepository = basketScope.ServiceProvider.GetRequiredService<IBasketRepository>();

        // Create initial basket
        var basket = new ShoppingCart { UserName = "testuser" };
        await basketRepository.CreateBasketAsync(basket, CancellationToken.None);

        var command = new AddItemToBasketCommand(
            UserName: "testuser",
            CartItem: new ShoppingCartItem
            {
                ProductId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                ProductName = "iPhone 15 Pro",
                Price = 1000,
                Quantity = 1
            }
        );

        var handler = basketScope.ServiceProvider.GetRequiredService<AddItemToBasketCommandHandler>();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Cart.Items.Should().HaveCount(1);

        var item = result.Cart.Items.First();
        item.ProductName.Should().Be("iPhone 15 Pro");
        item.OriginalPrice.Should().Be(1000);
        item.TotalDiscount.Should().Be(100); // 10% of 1000
        item.Price.Should().Be(900); // 1000 - 100
    }

    [Fact]
    public async Task AddItemToBasket_WithCouponCode_ShouldApplyCodeBasedDiscount()
    {
        // Arrange: Setup code-based discount
        using var discountScope = _factory.DiscountServices.CreateScope();
        var discountContext = discountScope.ServiceProvider.GetRequiredService<DiscountContext>();

        var coupon = new Coupon
        {
            ProductName = "MacBook Pro",
            ProductId = "2",
            Description = "15% off with code",
            Type = DiscountType.Percentage,
            PercentageDiscount = 15,
            Status = CouponStatus.Active,
            CouponCode = "SAVE15",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30)
        };
        discountContext.Coupons.Add(coupon);
        await discountContext.SaveChangesAsync();

        // Arrange: Setup basket command with coupon code
        using var basketScope = _factory.BasketServices.CreateScope();
        var basketRepository = basketScope.ServiceProvider.GetRequiredService<IBasketRepository>();

        var basket = new ShoppingCart { UserName = "testuser2" };
        await basketRepository.CreateBasketAsync(basket, CancellationToken.None);

        var command = new AddItemToBasketCommand(
            UserName: "testuser2",
            CartItem: new ShoppingCartItem
            {
                ProductId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                ProductName = "MacBook Pro",
                Price = 2000,
                Quantity = 1,
                CouponCodes = new List<string> { "SAVE15" }
            }
        );

        var handler = basketScope.ServiceProvider.GetRequiredService<AddItemToBasketCommandHandler>();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var item = result.Cart.Items.First();
        item.TotalDiscount.Should().Be(300); // 15% of 2000
        item.Price.Should().Be(1700); // 2000 - 300
    }

    [Fact]
    public async Task AddItemToBasket_WithNoDiscount_ShouldKeepOriginalPrice()
    {
        // Arrange: No discount setup
        using var basketScope = _factory.BasketServices.CreateScope();
        var basketRepository = basketScope.ServiceProvider.GetRequiredService<IBasketRepository>();

        var basket = new ShoppingCart { UserName = "testuser3" };
        await basketRepository.CreateBasketAsync(basket, CancellationToken.None);

        var command = new AddItemToBasketCommand(
            UserName: "testuser3",
            CartItem: new ShoppingCartItem
            {
                ProductId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                ProductName = "Product Without Discount",
                Price = 500,
                Quantity = 1
            }
        );

        var handler = basketScope.ServiceProvider.GetRequiredService<AddItemToBasketCommandHandler>();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var item = result.Cart.Items.First();
        item.TotalDiscount.Should().Be(0);
        item.Price.Should().Be(500); // No discount applied
    }

    [Fact]
    public async Task AddItemToBasket_WithStackableDiscounts_ShouldApplyMultipleDiscounts()
    {
        // Arrange: Setup multiple stackable discounts
        using var discountScope = _factory.DiscountServices.CreateScope();
        var discountContext = discountScope.ServiceProvider.GetRequiredService<DiscountContext>();

        var automaticCoupon = new Coupon
        {
            ProductName = "Gaming Laptop",
            ProductId = "4",
            Description = "5% automatic discount",
            Type = DiscountType.Percentage,
            PercentageDiscount = 5,
            Status = CouponStatus.Active,
            IsAutomatic = true,
            IsStackable = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        var codeCoupon = new Coupon
        {
            ProductName = "Gaming Laptop",
            ProductId = "4",
            Description = "10% with code",
            Type = DiscountType.Percentage,
            PercentageDiscount = 10,
            Status = CouponStatus.Active,
            CouponCode = "GAME10",
            IsStackable = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        discountContext.Coupons.AddRange(automaticCoupon, codeCoupon);
        await discountContext.SaveChangesAsync();

        // Arrange: Setup basket command
        using var basketScope = _factory.BasketServices.CreateScope();
        var basketRepository = basketScope.ServiceProvider.GetRequiredService<IBasketRepository>();

        var basket = new ShoppingCart { UserName = "testuser4" };
        await basketRepository.CreateBasketAsync(basket, CancellationToken.None);

        var command = new AddItemToBasketCommand(
            UserName: "testuser4",
            CartItem: new ShoppingCartItem
            {
                ProductId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                ProductName = "Gaming Laptop",
                Price = 1500,
                Quantity = 1,
                CouponCodes = new List<string> { "GAME10" }
            }
        );

        var handler = basketScope.ServiceProvider.GetRequiredService<AddItemToBasketCommandHandler>();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var item = result.Cart.Items.First();
        // First 5% automatic: 1500 * 0.05 = 75, remaining 1425
        // Then 10% code: 1425 * 0.10 = 142.5, total discount = 217.5
        item.TotalDiscount.Should().BeGreaterThan(200);
        item.Price.Should().BeLessThan(1300);
    }

    [Fact]
    public async Task AddItemToBasket_WithExpiredCoupon_ShouldNotApplyDiscount()
    {
        // Arrange: Setup expired discount
        using var discountScope = _factory.DiscountServices.CreateScope();
        var discountContext = discountScope.ServiceProvider.GetRequiredService<DiscountContext>();

        var expiredCoupon = new Coupon
        {
            ProductName = "Old Product",
            ProductId = "5",
            Description = "Expired 20% off",
            Type = DiscountType.Percentage,
            PercentageDiscount = 20,
            Status = CouponStatus.Expired,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-1)
        };

        discountContext.Coupons.Add(expiredCoupon);
        await discountContext.SaveChangesAsync();

        // Arrange: Setup basket command
        using var basketScope = _factory.BasketServices.CreateScope();
        var basketRepository = basketScope.ServiceProvider.GetRequiredService<IBasketRepository>();

        var basket = new ShoppingCart { UserName = "testuser5" };
        await basketRepository.CreateBasketAsync(basket, CancellationToken.None);

        var command = new AddItemToBasketCommand(
            UserName: "testuser5",
            CartItem: new ShoppingCartItem
            {
                ProductId = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                ProductName = "Old Product",
                Price = 800,
                Quantity = 1
            }
        );

        var handler = basketScope.ServiceProvider.GetRequiredService<AddItemToBasketCommandHandler>();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var item = result.Cart.Items.First();
        item.TotalDiscount.Should().Be(0); // Expired coupon should not apply
        item.Price.Should().Be(800);
    }
}

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// Configures both Basket and Discount services with in-memory databases
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public IServiceProvider BasketServices { get; private set; } = null!;
    public IServiceProvider DiscountServices { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Configure Basket API with in-memory database
            services.RemoveAll(typeof(DbContextOptions<Basket.API.Data.BasketContext>));
            services.AddDbContext<Basket.API.Data.BasketContext>(options =>
            {
                options.UseInMemoryDatabase("BasketTestDb_" + Guid.NewGuid());
            });

            // Configure Discount service with in-memory database
            services.RemoveAll(typeof(DbContextOptions<DiscountContext>));
            services.AddDbContext<DiscountContext>(options =>
            {
                options.UseInMemoryDatabase("DiscountTestDb_" + Guid.NewGuid());
            });

            // Build temporary service provider to initialize databases
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;

            // Initialize Basket database
            var basketDb = scopedServices.GetRequiredService<Basket.API.Data.BasketContext>();
            basketDb.Database.EnsureCreated();

            // Initialize Discount database
            var discountDb = scopedServices.GetRequiredService<DiscountContext>();
            discountDb.Database.EnsureCreated();

            // Store service providers for test access
            BasketServices = sp;
        });

        // Configure separate Discount service instance
        var discountBuilder = new WebHostBuilder()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddDbContext<DiscountContext>(options =>
                {
                    options.UseInMemoryDatabase("DiscountTestDb_" + Guid.NewGuid());
                });
                services.AddScoped<Discount.Grpc.Services.DiscountCalculator>();
            });

        DiscountServices = discountBuilder.Build().Services;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            (BasketServices as IDisposable)?.Dispose();
            (DiscountServices as IDisposable)?.Dispose();
        }
        base.Dispose(disposing);
    }
}
