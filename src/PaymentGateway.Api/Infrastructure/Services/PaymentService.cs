using OneOf;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Repositories;
using PaymentGateway.Api.Domain.Services;

namespace PaymentGateway.Api.Infrastructure.Services;

public class PaymentService(AcquiringBankPaymentService acquiringBankPaymentService, IPaymentRepository repository) : IPaymentService
{
    public async Task<OneOf<PaymentResult, DownstreamResponseError>> ProcessPayment(PaymentRequest paymentRequest)
    {
        var request = new AcquiringBankPaymentRequest(
            paymentRequest.CardNumber.ToString(),
            $"{paymentRequest.ExpiryMonth:D2}/{paymentRequest.ExpiryYear}",
            paymentRequest.Currency,
            paymentRequest.Amount,
            paymentRequest.Cvv.ToString()
        );

        var result = await acquiringBankPaymentService.ProcessPayment(request);

        return result.Match<OneOf<PaymentResult, DownstreamResponseError>>
        (
            success =>
            {
                var payment = new Payment(
                    Guid.NewGuid(),
                    success.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
                    paymentRequest.CardNumber.ToString()[(paymentRequest.CardNumber.ToString().Length - 4)..],
                    paymentRequest.ExpiryMonth,
                    paymentRequest.ExpiryYear,
                    paymentRequest.Currency,
                    paymentRequest.Amount
                );
                
                repository.Add(payment);
                return new PaymentResult(payment);
            },
            error => new DownstreamResponseError()
        );


    }
}