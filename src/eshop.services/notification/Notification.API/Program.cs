using BuildingBlocks.Messaging.MassTransit;
using BuildingBlocks.Middlewares;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Notification.API.Models;
using Notification.API.Services;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add EmailSettings
builder.Services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

// Add EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

// Add MassTransit with RabbitMQ
builder.Services.AddMessageBroker(configuration, typeof(Program).Assembly);

// Add Controllers
builder.Services.AddControllers();

// Add Health Checks
builder.Services.AddHealthChecks();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHealthChecks("/health", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

namespace Notification.API
{
    public partial class Program { }
}
