namespace Ordering.Domain.Exceptions
{
    public class OrderNotFoundException : DomainException
    {
        public OrderNotFoundException(Guid orderId) : base($"Order with Id = '{orderId}' not found.")
        {
        }
    }
}