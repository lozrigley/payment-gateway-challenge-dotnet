using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Application.Enums;
using PaymentGateway.Api.Application.Models.Requests;
using PaymentGateway.Api.Application.Models.Responses;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Repositories;
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
            ? await Task.FromResult(new OkObjectResult(new PostPaymentResponse
                {
                    Id = payment.Id,
                    Status = payment.Status.ToApplicationStatus(),
                    LastFourCardDigits = payment.LastFourCardDigits,
                    ExpiryMonth = payment.ExpiryMonth,
                    ExpiryYear = payment.ExpiryYear,
                    Currency = payment.Currency,
                    Amount = payment.Amount
                }))
               : await Task.FromResult(new NotFoundResult());
    }
    
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> ProcessPaymentAsync([FromBody] PostPaymentRequest request, [FromServices] IPaymentService paymentService)
    {
        var paymentRequest = new PaymentRequest(
            request.CardNumber,
            request.ExpiryMonth,
            request.ExpiryYear,
            request.Cvv,
            request.Currency,
            request.Amount);
        var result = await paymentService.ProcessPayment(paymentRequest);
        
        return result.Match(
            success =>
            {
                var payment = new PostPaymentResponse
                {
                    Id = success.Payment.Id,
                    Status = success.Payment.Status.ToApplicationStatus(),
                    LastFourCardDigits = success.Payment.LastFourCardDigits,
                    ExpiryMonth = success.Payment.ExpiryMonth,
                    ExpiryYear = success.Payment.ExpiryYear,
                    Currency = success.Payment.Currency,
                    Amount = success.Payment.Amount
                };


                return Ok(payment);
            },
            error => StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An unexpected downstream error occurred while processing the request."
            }));
    }
}


