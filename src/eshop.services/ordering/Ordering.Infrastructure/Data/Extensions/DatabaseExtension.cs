using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ordering.Infrastructure.Data.Extensions;

/// <summary>
/// Provides extension methods for managing and initializing the database within the
/// Ordering.Infrastructure context.
/// </summary>
/// <remarks>
/// The <c>DatabaseExtension</c> class contains methods to automatically apply
/// database migrations and seed the database with initial data. It is intended to be
/// used during application startup to ensure the database is properly configured and
/// populated.
/// </remarks>
public static class DatabaseExtension
{
    /// <summary>
    /// Initializes the database by applying migrations and seeding initial data.
    /// </summary>
    /// <param name="serviceProvider">
    /// An <see cref="IServiceProvider"/> instance used to create a scoped service and
    /// resolve the required dependencies for database initialization.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation of initializing the database.
    /// </returns>
    public static async Task InitialiseDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetService<OrderingDbContext>();
        await context?.Database.MigrateAsync()!;
        await SeedDatabaseAsync(context);
    }

    /// <summary>
    /// Seeds the database with initial data, including customers, products, orders, and order items.
    /// </summary>
    /// <param name="context">
    /// The <see cref="OrderingDbContext"/> instance representing the database context
    /// to be used for seeding initial data.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation of seeding the database.
    /// </returns>
    private static async Task SeedDatabaseAsync(OrderingDbContext context)
    {
        await SeedCustomerAsync(context);
        await SeedProductAsync(context);
        await SeedOrderAndItemAsync(context);
    }

    /// <summary>
    /// Seeds the database with initial customer data if no customers exist in the database.
    /// </summary>
    /// <param name="context">
    /// The <see cref="OrderingDbContext"/> instance providing access to the database context.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation of seeding customer data.
    /// </returns>
    private static async Task SeedCustomerAsync(OrderingDbContext context)
    {
        if (!await context.Customers.AnyAsync())
        {
            await context.Customers.AddRangeAsync(InitialData.Customers);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds the product data in the database if no product records exist.
    /// </summary>
    /// <param name="context">
    /// The <see cref="OrderingDbContext"/> instance used to interact with the database and add product records.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation of seeding the product data.
    /// </returns>
    private static async Task SeedProductAsync(OrderingDbContext context)
    {
        if (!await context.Products.AnyAsync())
        {
            await context.Products.AddRangeAsync(InitialData.Products);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds the database with initial order and order item data if no orders currently exist in the database.
    /// </summary>
    /// <param name="context">
    /// An instance of <see cref="OrderingDbContext"/> that provides access to the database context
    /// for seeding order and order item data.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation of seeding orders and order items into the database.
    /// </returns>
    private static async Task SeedOrderAndItemAsync(OrderingDbContext context)
    {
        if (!await context.Orders.AnyAsync())
        {
            await context.Orders.AddRangeAsync(InitialData.OrdersWithItems);
            await context.SaveChangesAsync();
        }
    }
}