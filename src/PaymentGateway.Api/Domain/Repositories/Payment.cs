﻿

namespace PaymentGateway.Api.Domain.Repositories;

public record Payment
{
    public Guid Id { get; set; }
    
    public PaymentStatus Status { get; set; }
    public int CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}

public enum PaymentStatus
{
    Authorized,
    Declined,
    Rejected
}
