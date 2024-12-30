

namespace PaymentGateway.Api.Domain.Repositories;

public interface IPaymentRepository
{
    void Add(Payment payment);
    Payment? Get(Guid id);
}