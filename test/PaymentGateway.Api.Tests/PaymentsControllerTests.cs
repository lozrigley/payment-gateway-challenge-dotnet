using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Application.Controllers;
using PaymentGateway.Api.Application.Models.Requests;
using PaymentGateway.Api.Application.Models.Responses;
using PaymentGateway.Api.Domain.Repositories;
using PaymentGateway.Api.Infrastructure.Repositories;
using FluentAssertions;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    
    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton<IPaymentRepository>(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("Cvv", "99")]
    [InlineData("Cvv", "99999")]
    [InlineData("Cvv", "what")]
    [InlineData("CardNumber", "1234567890123")]
    [InlineData("CardNumber", "12345678901234567890")]
    [InlineData("CardNumber", "1234567890a")]
    [InlineData("ExpiryMonth", "13")]
    [InlineData("ExpiryYear", "2020")]
    [InlineData("Currency", "AUD")]
    [InlineData("Amount", "0")]
    [InlineData("Amount", "-10")]
    
    public async Task PostAnInvalidPaymentReturns400WithErrorMessage(string property, string invalidOverride)
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        string json = $@"                      {{
                          ""CardNumber"": ""1234567812345678"",
                          ""ExpiryMonth"": 12,
                          ""ExpiryYear"": {DateTime.Now.Year + 2},
                          ""Cvv"": ""123"",
                          ""Currency"": ""GBP"",
                          ""Amount"": 100
                      }}";
        
        var request = JsonNode.Parse(json);
        
        if (request?[property] is not null)
        {
            request[property] = invalidOverride;
        }
        else
        {
            throw new ArgumentException("Test did not override value as expected");
        }
        
        //Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        
        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    }
    
    [Theory]
    [InlineData("Cvv", "999")]
    [InlineData("Cvv", "9999")]
    [InlineData("CardNumber", "12345678901234")]
    [InlineData("CardNumber", "1234567890123456789")]
    [InlineData("ExpiryMonth", "12")]
    [InlineData("Currency", "USD")]
    [InlineData("Currency", "EUR")]
    [InlineData("Amount", "1")]
    [InlineData("Amount", "9999999")]
    
    public async Task PostValidPaymentReturns200(string property, string @override)
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        string json = $@"                      {{
                          ""CardNumber"": ""1234567812345678"",
                          ""ExpiryMonth"": 12,
                          ""ExpiryYear"": {DateTime.Now.Year + 2},
                          ""Cvv"": ""123"",
                          ""Currency"": ""GBP"",
                          ""Amount"": 100
                      }}";
        
        var request = JsonNode.Parse(json);
        
        if (request?[property] is not null)
        {
            request[property] = @override;
        }
        else
        {
            throw new ArgumentException("Test did not override value as expected");
        }
        
        //Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        
        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}