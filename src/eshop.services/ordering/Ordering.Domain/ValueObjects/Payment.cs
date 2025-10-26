namespace Ordering.Domain.ValueObjects;

public record Payment
{
    public string CardName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string Expiration { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
    public int PaymentMethod  { get; set; }
    
    protected Payment()
    {
        
    }

    private Payment(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
    {
        CardName = cardName;
        CardNumber = cardNumber;
        Expiration = expiration;
        CVV = cvv;
        PaymentMethod = paymentMethod;
    }

    public static Payment Of(string cardName, string cardNumber, string expiration, string cvv, int paymentMethod)
    {
        ArgumentNullException.ThrowIfNull(cardName);
        ArgumentNullException.ThrowIfNull(cardNumber);
        ArgumentNullException.ThrowIfNull(expiration);
        ArgumentNullException.ThrowIfNull(cvv);
        ArgumentOutOfRangeException.ThrowIfNotEqual(cvv.Length, 3, "CVV must be 3 digits");
        
        return new Payment(cardName, cardNumber, expiration, cvv, paymentMethod);
    }
}
