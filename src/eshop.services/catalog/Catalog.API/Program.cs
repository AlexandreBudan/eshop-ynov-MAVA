using BuildingBlocks.Behaviors;
using BuildingBlocks.Middlewares;
using Catalog.API.Data;
using FluentValidation;
using HealthChecks.UI.Client;
using Marten;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();

// Mediator Pattern - CQRS
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));

});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Management PostGreSQl as NOSQL
builder.Services.AddMarten(options =>
    {
        options.Connection(configuration.GetConnectionString("CatalogConnection") ?? string.Empty);
    })
    .UseLightweightSessions();

// Initiate Database
if(builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<CatalogInitialData>();

// Health Check
builder.Services.AddHealthChecks()
    .AddNpgSql(configuration.GetConnectionString("CatalogConnection")!);

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

// Global Exception
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Health check Endpoint
app.UseHealthChecks("/health", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();