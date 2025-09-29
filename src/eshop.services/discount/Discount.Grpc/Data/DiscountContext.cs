using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data;

public sealed class DiscountContext(DbContextOptions<DiscountContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>().ToTable("Coupon")
            .HasData([
                new Coupon {Id = 1, ProductName = "IPhone X", Description = "IPhone X New", Amount = 150.0},
                new Coupon {Id = 2, ProductName = "Samsung 10", Description = "Samsung 10 New", Amount = 100.0}   
            ]);
    }
}