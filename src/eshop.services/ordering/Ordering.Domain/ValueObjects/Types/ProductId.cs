using Ordering.Domain.Exceptions;

namespace Ordering.Domain.ValueObjects.Types;

public record ProductId
{
    public Guid Value { get; set; }

    private ProductId (Guid value)
    {
        Value = value;   
    }

    public static ProductId Of(Guid value)
    {
        if(value == Guid.Empty)
            throw new DomainException("ProductId cannot be empty");
        
        return new ProductId(value);  
    }}