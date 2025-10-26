namespace Ordering.Domain.Enums;

/// <summary>
/// Represents the various states an order can be in during its lifecycle.
/// </summary>
public enum OrderStatus
{
    Draft = 1,
    Pending,
    Submitted,
    Cancelled,
    Confirmed,
    Completed,
    Shipped,
    Delivered
}