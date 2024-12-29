using PaymentGateway.Api.Application.Models.Responses;
using PaymentGateway.Api.Domain.Services;

namespace PaymentGateway.Api.Infrastructure.Repositories;

public class PaymentsRepository : IPaymentRepository
{
    public List<PostPaymentResponse> Payments = new();
    
    public void Add(PostPaymentResponse payment)
    {
        Payments.Add(payment);
    }

    public PostPaymentResponse Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }
}