using PaymentGateway.Api.Application.Models.Responses;

namespace PaymentGateway.Api.Domain.Services;

public interface IPaymentRepository
{
    void Add(PostPaymentResponse payment);
    PostPaymentResponse? Get(Guid id);
}