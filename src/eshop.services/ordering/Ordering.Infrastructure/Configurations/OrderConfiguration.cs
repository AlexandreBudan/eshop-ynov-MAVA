using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Enums;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion(
            id => id.Value,
            dbId => OrderId.Of(dbId)
        );
        
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .IsRequired();

        builder.HasMany<OrderItem>()
            .WithOne()
            .HasForeignKey(c => c.OrderId);

        builder.ComplexProperty(o => o.OrderName, nameBuilder =>
        {
            nameBuilder.Property(n => n.Value)
                .HasColumnName("OrderName")
                .HasMaxLength(100)
                .IsRequired();
        });
        
        builder.ComplexProperty(o => o.ShippingAddress, addressBuilder =>
        {
            addressBuilder.Property(a => a.FirstName)
                .HasMaxLength(50)
                .IsRequired();
            
            addressBuilder.Property(a => a.LastName)
                .HasMaxLength(50)
                .IsRequired();
            
            addressBuilder.Property(a => a.EmailAddress)
                .HasMaxLength(50);
            
            addressBuilder.Property(a => a.AddressLine)
                .HasMaxLength(180)
                .IsRequired();
            
            addressBuilder.Property(a => a.Country)
                .HasMaxLength(50)
                .IsRequired();
            
            addressBuilder.Property(a => a.State)
                .HasMaxLength(50);
            
            addressBuilder.Property(a => a.ZipCode)
                .HasMaxLength(5)
                .IsRequired();
        });
        
        builder.ComplexProperty(o => o.BillingAddress, addressBuilder =>
        {
            addressBuilder.Property(a => a.FirstName)
                .HasMaxLength(50)
                .IsRequired();
            
            addressBuilder.Property(a => a.LastName)
                .HasMaxLength(50)
                .IsRequired();
            
            addressBuilder.Property(a => a.EmailAddress)
                .HasMaxLength(50);
            
            addressBuilder.Property(a => a.AddressLine)
                .HasMaxLength(180)
                .IsRequired();
            
            addressBuilder.Property(a => a.Country)
                .HasMaxLength(50)
                .IsRequired();
            
            addressBuilder.Property(a => a.State)
                .HasMaxLength(50);
            
            addressBuilder.Property(a => a.ZipCode)
                .HasMaxLength(5)
                .IsRequired();
        });
        
        builder.ComplexProperty(o => o.Payment, addressBuilder =>
        {
            addressBuilder.Property(a => a.CardName)
                .HasMaxLength(50)
                .IsRequired();
            
            addressBuilder.Property(a => a.CardNumber)
                .HasMaxLength(24)
                .IsRequired();
            
            addressBuilder.Property(a => a.Expiration)
                .HasMaxLength(10)
                .IsRequired();
            
            addressBuilder.Property(a => a.CVV)
                .HasMaxLength(3)
                .IsRequired();
            
            addressBuilder.Property(a => a.PaymentMethod);
        });
        
        builder.Property(c => c.OrderStatus)
            .HasDefaultValue(OrderStatus.Draft)
            .HasConversion(s => s.ToString(), dbStatus => Enum.Parse<OrderStatus>(dbStatus))
            .IsRequired();
        
        builder.Property(c => c.TotalPrice).IsRequired();
        

    }
}