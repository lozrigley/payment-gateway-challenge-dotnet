using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Domain.Repositories;
using PaymentGateway.Api.Domain.Services;
using PaymentGateway.Api.Infrastructure.Repositories;
using PaymentGateway.Api.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentRepository, PaymentsRepository>();
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddHttpClient<AcquiringBankPaymentService>()
    .ConfigureHttpClient(x => x.BaseAddress = new Uri(builder.Configuration["AppSettings:AcquiringBankBaseUri"]!));
// builder.Services.Configure<ApiBehaviorOptions>(options =>
// {
//     options.SuppressModelStateInvalidFilter = true;
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
