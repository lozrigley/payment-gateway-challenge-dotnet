using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Application.Models.Responses;
using PaymentGateway.Api.Domain.Services;

namespace PaymentGateway.Api.Application.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentRepository paymentsRepository) : Controller
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = paymentsRepository.Get(id);
        return payment is not null 
            ? await Task.FromResult(new OkObjectResult(payment))
               : await Task.FromResult(new NotFoundResult());
    }
}
