using Ordering.Domain.Abstractions;
using Ordering.Domain.Enums;
using Ordering.Domain.Events;
using Ordering.Domain.ValueObjects;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Domain.Models;

/// <summary>
/// Represents an order with items, customer information, shipping and billing details,
/// payment details, and order status.
/// </summary>
public class Order : Aggregate<OrderId>
{
    private readonly List<OrderItem> _orderItems = new();
    
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();
    
    public CustomerId CustomerId { get; private set; } = null!;

    public OrderName OrderName { get; private set; } = null!;
    
    public Address ShippingAddress { get; private set; } = null!;

    public Address BillingAddress { get; private set; } = null!;
    
    public Payment Payment { get; private set; } = null!;

    public OrderStatus OrderStatus { get; private set; } = OrderStatus.Pending;

    public decimal TotalPrice
    {
        get => _orderItems.Sum(x => x.Price * x.Quantity);
        private set
        {
        }
    }

    /// <summary>
    /// Adds an item to the order.
    /// </summary>
    /// <param name="productId">The unique identifier of the product to be added to the order.</param>
    /// <param name="quantity">The quantity of the product to be added. Must be greater than zero.</param>
    /// <param name="price">The price per unit of the product to be added. Must be greater than zero.</param>
    public void AddOrderItem(ProductId productId, int quantity, decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity, "Quantity must be greater than 0");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price, "Price must be greater than 0");
        
        _orderItems.Add(new OrderItem(productId, Id, price, quantity));
    }

    /// <summary>
    /// Removes an item from the order based on the specified product identifier.
    /// </summary>
    /// <param name="productId">The unique identifier of the product to be removed from the order.</param>
    public void RemoveOrderItem(ProductId productId)
    {
        var orderItem = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (orderItem is not null)
        {
            _orderItems.Remove(orderItem);
        }
    }

    /// <summary>
    /// Creates a new order with the specified customer details, shipping and billing addresses,
    /// payment information, and order name.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer placing the order.</param>
    /// <param name="orderName">The name or description associated with the order.</param>
    /// <param name="shippingAddress">The shipping address where the order will be delivered.</param>
    /// <param name="billingAddress">The billing address associated with the order payment.</param>
    /// <param name="payment">The payment details for the order.</param>
    /// <returns>A new instance of the Order class with the specified details.</returns>
    public static Order Create(CustomerId customerId, OrderName orderName, Address shippingAddress,
        Address billingAddress, Payment payment)
    {
        var order = new Order
        {
            Id = OrderId.Of(Guid.NewGuid()),
            CustomerId = customerId,
            OrderName = orderName,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Payment = payment,
            OrderStatus = OrderStatus.Pending,
        };
       
       order.AddDomainEvent(new OrderCreatedEvent(order));
       
       return order;
    }

    /// <summary>
    /// Updates the details of the order including name, shipping and billing addresses, payment details, and status.
    /// </summary>
    /// <param name="orderName">The new name of the order.</param>
    /// <param name="shippingAddress">The new shipping address for the order.</param>
    /// <param name="orderStatus">The new status of the order.</param>
    /// <param name="billingAddress">The new billing address for the order.</param>
    /// <param name="payment">The updated payment details for the order.</param>
    public void Update(OrderName orderName, Address shippingAddress, OrderStatus orderStatus,
        Address billingAddress, Payment payment)
    {
        OrderName = orderName;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        Payment = payment;
        OrderStatus  = orderStatus;
        
        AddDomainEvent(new OrderUpdatedEvent(this));

    }
}