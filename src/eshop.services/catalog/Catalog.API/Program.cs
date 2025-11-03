using BuildingBlocks.Behaviors;
using BuildingBlocks.Middlewares;
using Catalog.API.Data;
using Catalog.API.Services;
using Discount.Grpc;
using FluentValidation;
using HealthChecks.UI.Client;
using Marten;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;

// Enable HTTP/2 without TLS for gRPC in development (h2c protocol)
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel with separate endpoints for HTTP/1.1 (REST) and HTTP/2 (gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    // Port 6060: REST API with HTTP/1.1
    options.ListenAnyIP(6060, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    // Port 6006: gRPC with HTTP/2 (without TLS for dev)
    options.ListenAnyIP(6006, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(configuration.GetValue<string>("GrpcSettings:DiscountUrl") ?? string.Empty);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

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

// Map gRPC service
app.MapGrpcService<CatalogServiceServer>();

app.MapGet("/grpc",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// Global Exception
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Health check Endpoint
app.UseHealthChecks("/health", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();