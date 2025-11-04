using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Data;

public interface IOrderingDbContext
{
    DbSet<Customer> Customers { get; }

    DbSet<Order> Orders { get;}

    DbSet<Product> Products { get;}

    DbSet<OrderItem> OrderItems { get; }

    DbSet<FailedEmail> FailedEmails { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

}