using System.Collections.Concurrent;

using PaymentGateway.Api.Application.Models.Responses;
using PaymentGateway.Api.Domain.Repositories;

namespace PaymentGateway.Api.Infrastructure.Repositories;

public class PaymentsRepository : IPaymentRepository
{
    //private for encapsulation and Concurrent bag for thread safety
    private readonly ConcurrentBag<Payment?> _payments = new();

    public void Add(Payment payment)
    {
        _payments.Add(payment);
    }

    public Payment? Get(Guid id)
    {
        return _payments?.FirstOrDefault(p => p?.Id == id);
    }
}