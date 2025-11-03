using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Services;
using Ordering.Infrastructure.Data;
using Ordering.Infrastructure.Data.Interceptors;
using Ordering.Infrastructure.Services;

namespace Ordering.Infrastructure;

public static class ServiceExtension
{
    public static IServiceCollection AddInfraStructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderingConnection");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<IOrderingDbContext, OrderingDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        // Register email service
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}

/// Notes : dotnet ef migrations add InitiateOrderingDatabase --startup-project ../Ordering.API -o Data/Migrations
/// dotnet ef database update --startup-project ../Ordering.API