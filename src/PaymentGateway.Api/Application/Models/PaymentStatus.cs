namespace PaymentGateway.Api.Application.Enums;

public enum PaymentStatus
{
    Authorized,
    Declined
}

public static class PaymentStatusHelper
{
    public static PaymentStatus ToApplicationStatus(this PaymentGateway.Api.Domain.PaymentStatus status)
        => (PaymentStatus)(int)status;
    
    public static PaymentGateway.Api.Domain.PaymentStatus ToDomainStatus(this PaymentStatus status)
    => (PaymentGateway.Api.Domain.PaymentStatus)(int)status;
}