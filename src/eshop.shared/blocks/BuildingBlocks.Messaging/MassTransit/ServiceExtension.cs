using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.MassTransit;

public static class ServiceExtension
{
    /// <summary>
    /// Configures and registers MassTransit with RabbitMQ as the message broker in the application.
    /// </summary>
    /// <param name="services">The IServiceCollection to which the message broker dependencies will be added.</param>
    /// <param name="configuration">The application configuration used to retrieve RabbitMQ connection details such as host, username, and password.</param>
    /// <param name="assembly">
    /// An optional assembly that contains consumer implementations.
    /// If provided, consumers will be registered automatically from the specified assembly.
    /// </param>
    /// <returns>The IServiceCollection with the added message broker services.</returns>
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration,
        Assembly? assembly = null)
    {
        services.AddMassTransit(config =>
            {
                config.SetKebabCaseEndpointNameFormatter();
                if(assembly != null)
                    config.AddConsumers(assembly);
                
                config.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(configuration["MessageBroker:Host"]!), host =>
                    {
                        host.Username(configuration["MessageBroker:UserName"]!);
                        host.Password(configuration["MessageBroker:Password"]!);
                    });
                    
                    cfg.UseMessageRetry(r => r.Exponential(3, 
                        TimeSpan.FromSeconds(1), 
                        TimeSpan.FromMinutes(5), 
                        TimeSpan.FromSeconds(1)));
        
                    cfg.UseCircuitBreaker(cb =>
                    {
                        cb.TripThreshold = 15;
                        cb.ActiveThreshold = 10;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                    });
                    
                    cfg.ConfigureEndpoints(context);
                });
            }
        );
        return services;
    }
}