using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data.Extensions;

/// <summary>
/// Provides an extension method to handle database migrations
/// using the application's dependency injection context.
/// </summary>
public static class MigrationExtension
{
    /// <summary>
    /// Applies pending migrations for the database context if they exist
    /// and ensures the database is up to date. This method is typically used
    /// during application startup to handle database schema updates.
    /// </summary>
    /// <param name="app">The application builder instance used to configure the application's request pipeline.</param>
    /// <returns>Returns the <see cref="IApplicationBuilder"/> instance to allow for method chaining.</returns>
    public static IApplicationBuilder UseCustomMigration(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<DiscountContext>();
        dbContext.Database.MigrateAsync();
        
        return app;
    }
}