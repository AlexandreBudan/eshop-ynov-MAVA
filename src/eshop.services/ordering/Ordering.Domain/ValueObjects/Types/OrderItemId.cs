using Ordering.Domain.Exceptions;

namespace Ordering.Domain.ValueObjects.Types;

public record OrderItemId
{
    public Guid Value { get; set; }

    private OrderItemId (Guid value)
    {
        Value = value;   
    }

    public static OrderItemId Of(Guid value)
    {
        if(value == Guid.Empty)
            throw new DomainException("OrderItemId cannot be empty");
        
        return new OrderItemId(value);  
    }
}