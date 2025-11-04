using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Models;

namespace Ordering.Infrastructure.Configurations;

/// <summary>
/// Entity Framework configuration for FailedEmail entity.
/// </summary>
public class FailedEmailConfiguration : IEntityTypeConfiguration<FailedEmail>
{
    public void Configure(EntityTypeBuilder<FailedEmail> builder)
    {
        builder.ToTable("FailedEmails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrderId)
            .IsRequired();

        builder.Property(e => e.RecipientEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.EmailSubject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.EmailBody)
            .IsRequired();

        builder.Property(e => e.RetryCount)
            .IsRequired();

        builder.Property(e => e.MaxRetries)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastRetryAt);

        builder.Property(e => e.NextRetryAt);

        builder.Property(e => e.LastError)
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        // Indexes for performance
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => new { e.Status, e.NextRetryAt });
    }
}
