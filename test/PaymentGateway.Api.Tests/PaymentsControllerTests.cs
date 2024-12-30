using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Application.Controllers;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Domain.Repositories;
using PaymentGateway.Api.Infrastructure.Repositories;
using FluentAssertions;
using FluentAssertions.Execution;
using PaymentGateway.Api.Infrastructure.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    
    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new Payment(
            Id: Guid.NewGuid(),
            Status: PaymentStatus.Authorized,
            ExpiryMonth: _random.Next(1, 12),
            ExpiryYear: _random.Next(2024, 2030),
            Amount: _random.Next(1, 10000),
            LastFourCardDigits: _random.Next(1111, 9999).ToString(),
            Currency: "GBP"); 

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton<IPaymentRepository>(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<JsonDocument>();
        
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
    
    public async Task PostAnInvalidPaymentWithIncorrectPropertyValuesReturns400(string property, string invalidOverride)
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
    
    [Fact]
    public async Task PostValidPaymentReturns200AndPersistsAuthorizedPayment()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        string json = $@"                      {{
                          ""CardNumber"": ""2222405343248877"",
                          ""ExpiryMonth"": 4,
                          ""ExpiryYear"": 2025,
                          ""Cvv"": ""123"",
                          ""Currency"": ""GBP"",
                          ""Amount"": 100
                      }}";
        
        var request = JsonNode.Parse(json);
        
        //Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        var jsonDoc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var id = jsonDoc!.RootElement.GetProperty("id").GetGuid();
        var getResponse = await client.GetAsync($"/api/Payments/{id}");
        var getJsonDoc = await getResponse.Content.ReadFromJsonAsync<JsonDocument>();
        
        //Assert
        using var scope = new AssertionScope();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        getJsonDoc!.RootElement.GetProperty("id").GetGuid().Should().Be(id);
        getJsonDoc.RootElement.GetProperty("status").GetString().Should().Be("Authorized");
        getJsonDoc.RootElement.GetProperty("lastFourCardDigits").GetString().Should().Be("8877");
        getJsonDoc.RootElement.GetProperty("expiryMonth").GetInt32().Should().Be(4);
        getJsonDoc.RootElement.GetProperty("expiryYear").GetInt32().Should().Be(2025);
        getJsonDoc.RootElement.GetProperty("currency").GetString().Should().Be("GBP");
        getJsonDoc.RootElement.GetProperty("amount").GetInt32().Should().Be(100);
    }
    
    [Fact]
    public async Task PostValidPaymentReturns200AndPersistsUnAuthorizedPayment()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        string json = $@"                      {{
                          ""CardNumber"": ""2222405343248112"",
                          ""ExpiryMonth"": 1,
                          ""ExpiryYear"": 2026,
                          ""Cvv"": ""456"",
                          ""Currency"": ""USD"",
                          ""Amount"": 60000
                      }}";
        
        var request = JsonNode.Parse(json);
        
        //Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        var jsonDoc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var id = jsonDoc!.RootElement.GetProperty("id").GetGuid();
        var getResponse = await client.GetAsync($"/api/Payments/{id}");
        var getJsonDoc = await getResponse.Content.ReadFromJsonAsync<JsonDocument>();
        
        //Assert
        using var scope = new AssertionScope();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        getJsonDoc!.RootElement.GetProperty("id").GetGuid().Should().Be(id);
        getJsonDoc.RootElement.GetProperty("status").GetString().Should().Be("Declined");
        getJsonDoc.RootElement.GetProperty("lastFourCardDigits").GetString().Should().Be("8112");
        getJsonDoc.RootElement.GetProperty("expiryMonth").GetInt32().Should().Be(1);
        getJsonDoc.RootElement.GetProperty("expiryYear").GetInt32().Should().Be(2026);
        getJsonDoc.RootElement.GetProperty("currency").GetString().Should().Be("USD");
        getJsonDoc.RootElement.GetProperty("amount").GetInt32().Should().Be(60000);
    }
    
    [Theory]
    [InlineData("Cvv")]
    [InlineData("CardNumber")]
    [InlineData("ExpiryMonth")]
    [InlineData("ExpiryYear")]
    [InlineData("Currency")]
    [InlineData("Amount")]
    
    public async Task PostAnInvalidWithMissingRequiredFieldsPaymentReturns400(string propertyToRemove)
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
        
        if (request?[propertyToRemove] is not null)
        {
            ((JsonObject)request).Remove(propertyToRemove);
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
    
    [Fact]
    //Not in spec but needed to handle edge case of third party api being unavailable
    public async Task PostPayment_AcquiringBankNotAvailable_Returns500Error()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var client = new HttpClient(new CustomHttpMessageHandler())
                    {
                        BaseAddress = new Uri("https://mocked-service")
                    };
                    
                    services.AddSingleton(new AcquiringBankPaymentService(client));
                });
            });
        var client = webApplicationFactory.CreateClient();

        string json = $@"                      {{
                          ""CardNumber"": ""2222405343248877"",
                          ""ExpiryMonth"": 4,
                          ""ExpiryYear"": 2025,
                          ""Cvv"": ""123"",
                          ""Currency"": ""GBP"",
                          ""Amount"": 100
                      }}";
        
        var request = JsonNode.Parse(json);
        
        //Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        
        //Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}

public class CustomHttpMessageHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        // Mock a response
        return new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("{\"message\": \"This is really bad\"}")
        };
    }
}