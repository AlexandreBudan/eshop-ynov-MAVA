using System.Reflection;
using Discount.Grpc.Data;
using Discount.Grpc.Data.Extensions;
using Discount.Grpc.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// Enable HTTP/2 without TLS for gRPC in development (h2c protocol)
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel with separate endpoints for HTTP/1.1 (REST) and HTTP/2 (gRPC)
builder.WebHost.ConfigureKestrel(options =>
{
    // Port 6062: REST API with HTTP/1.1
    options.ListenAnyIP(6062, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    // Port 6026: gRPC with HTTP/2 (without TLS for dev)
    options.ListenAnyIP(6026, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddScoped<DiscountCalculator>();

builder.Services.AddDbContext<DiscountContext>(options => options.UseSqlite(configuration.GetConnectionString("DiscountConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Discount Service API",
        Description = "API REST pour la gestion et l'application des réductions, coupons et campagnes promotionnelles",
        Contact = new OpenApiContact
        {
            Name = "E-Shop Team",
            Email = "support@eshop.com"
        }
    });

    // Include XML comments for better documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add authorization header (if needed in future)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCustomMigration();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Discount Service API V1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Discount Service API Documentation";
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<DiscountServiceServer>();

app.Run();