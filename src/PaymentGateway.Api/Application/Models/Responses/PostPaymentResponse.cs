using PaymentGateway.Api.Application.Enums;

namespace PaymentGateway.Api.Application.Models.Responses;

public class PostPaymentResponse 
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public string LastFourCardDigits { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}

