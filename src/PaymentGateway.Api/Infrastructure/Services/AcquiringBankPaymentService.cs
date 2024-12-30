using PaymentGateway.Api.Domain;
using OneOf;
namespace PaymentGateway.Api.Infrastructure.Services;

public class AcquiringBankPaymentService(HttpClient httpClient)
{
    public async Task<OneOf<AcquiringBankPaymentResponse,HttpErrorResponse>> ProcessPayment(AcquiringBankPaymentRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("payments", request);
        
        if (!response.IsSuccessStatusCode)
        {
            return new HttpErrorResponse((int)response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<AcquiringBankPaymentResponse>())!;
    }
}

public record AcquiringBankPaymentRequest(
    string card_number,
    string expiry_date,
    string currency,
    int amount,
    string Cvv
);

public record AcquiringBankPaymentResponse(
    bool Authorized,
    string AuthorizationCode
);

public record HttpErrorResponse(int StatusCode);