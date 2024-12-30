using OneOf;
namespace PaymentGateway.Api.Domain.Services;

public interface IPaymentService
{
    Task<OneOf<PaymentResult, DownstreamResponseError>> ProcessPayment(PaymentRequest payment);
}

public record PaymentResult(Payment Payment);

public record DownstreamResponseError();

