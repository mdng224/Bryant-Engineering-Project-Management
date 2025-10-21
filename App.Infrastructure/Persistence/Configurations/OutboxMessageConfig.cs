using App.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        // --- Table  ----------------------------------------------------
        b.ToTable("outbox_messages");
        
        // --- Key -------------------------------------------------------
        b.HasKey(om => om.Id);
        
        // --- Properties ------------------------------------------------
        b.Property(om => om.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(om => om.Type).HasColumnName("type").IsRequired().HasMaxLength(256);
        b.Property(om => om.Payload).HasColumnName("payload").IsRequired();
        b.Property(om => om.RetryCount).HasDefaultValue(0).IsRequired();
        b.Property(om => om.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .HasColumnType("timestamptz")
            .IsRequired();

        b.Property(om => om.ProcessedAtUtc)
            .HasColumnName("processed_at_utc")
            .HasColumnType("timestamptz");
        
        // --- Indexes ---------------------------------------------------
        // Common query: WHERE processed_at_utc IS NULL
        b.HasIndex(om => om.ProcessedAtUtc)
            .HasDatabaseName("ix_outbox_messages_processed_at_utc");
    }
}