using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Application.Features.Orders.EventHandlers.Domain;
using Ordering.Application.Services;
using Ordering.Domain.Enums;
using Ordering.Domain.Events;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;
using Ordering.Domain.ValueObjects.Types;
using Xunit;

namespace IntegrationTests;

/// <summary>
/// Unit tests for the order email confirmation flow.
/// Tests that emails are sent when orders are created.
/// </summary>
public class OrderEmailConfirmationTests
{
    [Fact]
    public async Task OrderCreatedEvent_WithValidCustomerEmail_ShouldSendConfirmationEmail()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var mockDbContext = new Mock<IOrderingDbContext>();
        var mockPublishEndpoint = new Mock<MassTransit.IPublishEndpoint>();
        var mockFeatureManager = new Mock<Microsoft.FeatureManagement.IFeatureManager>();
        var mockLogger = new Mock<ILogger<OrderCreatedEventHandler>>();

        var customerId = CustomerId.Of(Guid.NewGuid());
        var customer = Customer.Create(customerId, "test@example.com", "John Doe");

        var mockCustomers = new List<Customer> { customer }.AsQueryable();
        var mockCustomersDbSet = CreateMockDbSet(mockCustomers);

        mockDbContext.Setup(x => x.Customers).Returns(mockCustomersDbSet.Object);
        mockFeatureManager.Setup(x => x.IsEnabledAsync("OrderFulfilment"))
            .ReturnsAsync(false);

        var handler = new OrderCreatedEventHandler(
            mockPublishEndpoint.Object,
            mockFeatureManager.Object,
            mockLogger.Object,
            mockDbContext.Object,
            mockEmailService.Object
        );

        var order = Order.Create(
            customerId,
            OrderName.Of("Test Order"),
            Address.Of("John", "Doe", "test@example.com", "123 Test St", "USA", "CA", "90210"),
            Address.Of("John", "Doe", "test@example.com", "123 Test St", "USA", "CA", "90210"),
            Payment.Of("John Doe", "1234567890123456", "12/25", "123", 1)
        );

        order.AddOrderItem(
            ProductId.Of(Guid.NewGuid()),
            1,
            1000,
            "iPhone 15",
            "Latest iPhone",
            "iphone.jpg",
            100,
            900
        );

        var orderCreatedEvent = new OrderCreatedEvent(order);

        // Act
        await handler.Handle(orderCreatedEvent, CancellationToken.None);

        // Assert
        mockEmailService.Verify(
            x => x.SendOrderConfirmationEmailAsync(
                It.Is<OrderDto>(dto =>
                    dto.CustomerId == customerId.Value &&
                    dto.OrderItems.Count == 1 &&
                    dto.OrderItems[0].ProductName == "iPhone 15" &&
                    dto.OrderItems[0].DiscountAmount == 100 &&
                    dto.OrderItems[0].FinalPrice == 900
                ),
                "test@example.com",
                It.IsAny<CancellationToken>()
            ),
            Times.Once,
            "Email confirmation should be sent with correct order details including discount"
        );
    }

    [Fact]
    public async Task OrderCreatedEvent_WithoutCustomerEmail_ShouldNotSendEmail()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var mockDbContext = new Mock<IOrderingDbContext>();
        var mockPublishEndpoint = new Mock<MassTransit.IPublishEndpoint>();
        var mockFeatureManager = new Mock<Microsoft.FeatureManagement.IFeatureManager>();
        var mockLogger = new Mock<ILogger<OrderCreatedEventHandler>>();

        var customerId = CustomerId.Of(Guid.NewGuid());
        var customer = Customer.Create(customerId, null, "John Doe"); // No email

        var mockCustomers = new List<Customer> { customer }.AsQueryable();
        var mockCustomersDbSet = CreateMockDbSet(mockCustomers);

        mockDbContext.Setup(x => x.Customers).Returns(mockCustomersDbSet.Object);
        mockFeatureManager.Setup(x => x.IsEnabledAsync("OrderFulfilment"))
            .ReturnsAsync(false);

        var handler = new OrderCreatedEventHandler(
            mockPublishEndpoint.Object,
            mockFeatureManager.Object,
            mockLogger.Object,
            mockDbContext.Object,
            mockEmailService.Object
        );

        var order = Order.Create(
            customerId,
            OrderName.Of("Test Order"),
            Address.Of("John", "Doe", "test@example.com", "123 Test St", "USA", "CA", "90210"),
            Address.Of("John", "Doe", "test@example.com", "123 Test St", "USA", "CA", "90210"),
            Payment.Of("John Doe", "1234567890123456", "12/25", "123", 1)
        );

        var orderCreatedEvent = new OrderCreatedEvent(order);

        // Act
        await handler.Handle(orderCreatedEvent, CancellationToken.None);

        // Assert
        mockEmailService.Verify(
            x => x.SendOrderConfirmationEmailAsync(
                It.IsAny<OrderDto>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never,
            "Email should not be sent when customer email is missing"
        );
    }

    [Fact]
    public async Task OrderCreatedEvent_WithEmailServiceFailure_ShouldNotThrowException()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var mockDbContext = new Mock<IOrderingDbContext>();
        var mockPublishEndpoint = new Mock<MassTransit.IPublishEndpoint>();
        var mockFeatureManager = new Mock<Microsoft.FeatureManagement.IFeatureManager>();
        var mockLogger = new Mock<ILogger<OrderCreatedEventHandler>>();

        var customerId = CustomerId.Of(Guid.NewGuid());
        var customer = Customer.Create(customerId, "test@example.com", "John Doe");

        var mockCustomers = new List<Customer> { customer }.AsQueryable();
        var mockCustomersDbSet = CreateMockDbSet(mockCustomers);

        mockDbContext.Setup(x => x.Customers).Returns(mockCustomersDbSet.Object);
        mockFeatureManager.Setup(x => x.IsEnabledAsync("OrderFulfilment"))
            .ReturnsAsync(false);

        // Setup email service to throw exception
        mockEmailService
            .Setup(x => x.SendOrderConfirmationEmailAsync(
                It.IsAny<OrderDto>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP server unavailable"));

        var handler = new OrderCreatedEventHandler(
            mockPublishEndpoint.Object,
            mockFeatureManager.Object,
            mockLogger.Object,
            mockDbContext.Object,
            mockEmailService.Object
        );

        var order = Order.Create(
            customerId,
            OrderName.Of("Test Order"),
            Address.Of("John", "Doe", "test@example.com", "123 Test St", "USA", "CA", "90210"),
            Address.Of("John", "Doe", "test@example.com", "123 Test St", "USA", "CA", "90210"),
            Payment.Of("John Doe", "1234567890123456", "12/25", "123", 1)
        );

        var orderCreatedEvent = new OrderCreatedEvent(order);

        // Act & Assert - should not throw
        Func<Task> act = async () => await handler.Handle(orderCreatedEvent, CancellationToken.None);
        await act.Should().NotThrowAsync("Email failure should not break order creation");

        // Verify email was attempted
        mockEmailService.Verify(
            x => x.SendOrderConfirmationEmailAsync(
                It.IsAny<OrderDto>(),
                "test@example.com",
                It.IsAny<CancellationToken>()
            ),
            Times.Once,
            "Email sending should be attempted"
        );
    }

    [Fact]
    public async Task OrderCreatedEvent_WithMultipleItems_ShouldIncludeAllItemsInEmail()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var mockDbContext = new Mock<IOrderingDbContext>();
        var mockPublishEndpoint = new Mock<MassTransit.IPublishEndpoint>();
        var mockFeatureManager = new Mock<Microsoft.FeatureManagement.IFeatureManager>();
        var mockLogger = new Mock<ILogger<OrderCreatedEventHandler>>();

        var customerId = CustomerId.Of(Guid.NewGuid());
        var customer = Customer.Create(customerId, "test@example.com", "John Doe");

        var mockCustomers = new List<Customer> { customer }.AsQueryable();
        var mockCustomersDbSet = CreateMockDbSet(mockCustomers);

        mockDbContext.Setup(x => x.Customers).Returns(mockCustomersDbSet.Object);
        mockFeatureManager.Setup(x => x.IsEnabledAsync("OrderFulfilment"))
            .ReturnsAsync(false);

        var handler = new OrderCreatedEventHandler(
            mockPublishEndpoint.Object,
            mockFeatureManager.Object,
            mockLogger.Object,
            mockDbContext.Object,
            mockEmailService.Object
        );

        var order = Order.Create(
            customerId,
            OrderName.Of("Multi-Item Order"),
            Address.Of("John", "Doe", "test@example.com", "123 Test St", "USA", "CA", "90210"),
            Address.Of("John", "Doe", "test@example.com", "123 Test St", "USA", "CA", "90210"),
            Payment.Of("John Doe", "1234567890123456", "12/25", "123", 1)
        );

        // Add multiple items with discounts
        order.AddOrderItem(ProductId.Of(Guid.NewGuid()), 1, 1000, "iPhone 15", null, null, 100, 900);
        order.AddOrderItem(ProductId.Of(Guid.NewGuid()), 2, 500, "AirPods Pro", null, null, 50, 450);
        order.AddOrderItem(ProductId.Of(Guid.NewGuid()), 1, 300, "Apple Watch", null, null, 30, 270);

        var orderCreatedEvent = new OrderCreatedEvent(order);

        // Act
        await handler.Handle(orderCreatedEvent, CancellationToken.None);

        // Assert
        mockEmailService.Verify(
            x => x.SendOrderConfirmationEmailAsync(
                It.Is<OrderDto>(dto =>
                    dto.OrderItems.Count == 3 &&
                    dto.OrderItems[0].ProductName == "iPhone 15" &&
                    dto.OrderItems[1].ProductName == "AirPods Pro" &&
                    dto.OrderItems[2].ProductName == "Apple Watch" &&
                    dto.OrderItems.Sum(i => i.DiscountAmount ?? 0) == 230 // Total discount
                ),
                "test@example.com",
                It.IsAny<CancellationToken>()
            ),
            Times.Once,
            "Email should include all order items with their discounts"
        );
    }

    /// <summary>
    /// Helper method to create a mock DbSet
    /// </summary>
    private static Mock<Microsoft.EntityFrameworkCore.DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        return mockSet;
    }
}
