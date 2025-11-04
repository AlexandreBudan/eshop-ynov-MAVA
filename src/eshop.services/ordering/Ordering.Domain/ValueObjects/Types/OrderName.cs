using Ordering.Domain.Exceptions;

namespace Ordering.Domain.ValueObjects.Types;

public record OrderName
{
    private const int DefaultLenght = 5;
    public string Value { get; set; } = string.Empty;
    
    private OrderName(string value)
    {
        Value = value;
    }

    public static OrderName Of(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            throw new DomainException("OrderName cannot be empty");
        
        ArgumentOutOfRangeException.ThrowIfLessThan(value.Length, DefaultLenght, "OrderName must be 5 characters");
        
        return new OrderName(value);
    }
}