using BuildingBlocks.Middlewares;
using Catalog.Grpc;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ordering.API.Services;
using Ordering.Application.Services;

namespace Ordering.API.Extensions;

/// <summary>
/// Provides extension methods for configuring and using API-specific services and middleware components
/// within the Ordering API application.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Configures and registers API-specific services, middleware components, and health checks for the application.
    /// </summary>
    /// <param name="services">The service collection to which the services will be added.</param>
    /// <param name="configuration">The application configuration used to retrieve settings and connection strings.</param>
    /// <returns>The updated service collection with API-specific services registered.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        
        services.AddOpenApi();

        var connectionString = configuration.GetConnectionString("OrderingConnection");
        services.AddHealthChecks()
            .AddSqlServer(connectionString!);

        // Register gRPC client for Catalog service
        var catalogUrl = configuration["GrpcSettings:CatalogUrl"] ?? "http://localhost:6060";
        services.AddGrpcClient<CatalogProtoService.CatalogProtoServiceClient>(options =>
        {
            options.Address = new Uri(catalogUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            return handler;
        });

        services.AddScoped<ICatalogService, CatalogService>();
        return services;
    }

    /// <summary>
    /// Configures the application to use API-specific middleware components, health checks, and controllers.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The configured web application instance with API-specific middleware and components applied.</returns>
    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.UseMiddleware<ExceptionHandlerMiddleware>();

        app.UseHealthChecks("/health", new HealthCheckOptions()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapControllers();
        
        return app;
    }
}