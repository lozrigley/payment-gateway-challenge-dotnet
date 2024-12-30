

namespace PaymentGateway.Api.Domain;

public record Payment(
    Guid Id,
    PaymentStatus Status,
    string LastFourCardDigits,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount
);

public record PaymentRequest(
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv,
    string Currency,
    int Amount
);


public enum PaymentStatus
{
    Authorized,
    Declined
}

public record Success(Payment Payment);

public record Error();
