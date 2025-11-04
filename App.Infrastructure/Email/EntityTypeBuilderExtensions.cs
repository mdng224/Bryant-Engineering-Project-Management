using App.Domain.Common;
using App.Domain.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Email;

public static class EntityTypeBuilderExtensions
{
    public static void OwnsAddress<T>(this EntityTypeBuilder<T> builder)
        where T : class
    {
        builder.OwnsOne<Address>(
            nameof(Address),
            a =>
            {
                a.Property(p => p.Line1).HasColumnName("Address_Line_1").HasMaxLength(64);
                a.Property(p => p.Line2).HasColumnName("Address_Line_2").HasMaxLength(64);
                a.Property(p => p.City).HasMaxLength(50);
                a.Property(p => p.State).HasMaxLength(50);
                a.Property(p => p.PostalCode).HasMaxLength(15);
            });
    }
    
    /// <summary>
    /// Configures audit columns for entities implementing <see cref="IAuditableEntity"/>.
    /// </summary>
    public static void ConfigureAuditable<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IAuditableEntity
    {
        builder.Property(u => u.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired()
            .HasColumnType("timestamptz")
            .ValueGeneratedNever();

        builder.Property(u => u.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired()
            .HasColumnType("timestamptz")
            .ValueGeneratedNever();

        builder.Property(u => u.CreatedById)
            .HasColumnName("created_by_id")
            .HasColumnType("uuid")
            .IsRequired(false);

        builder.Property(u => u.UpdatedById)
            .HasColumnName("updated_by_id")
            .HasColumnType("uuid")
            .IsRequired(false);
    }
    
    /// <summary>
    /// Configures soft-deletion columns for entities implementing <see cref="ISoftDeletable"/>.
    /// </summary>
    public static void ConfigureSoftDeletable<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ISoftDeletable
    {
        builder.Property(e => e.DeletedAtUtc)
            .HasColumnName("deleted_at_utc")
            .HasColumnType("timestamptz");

        builder.Property(e => e.DeletedById)
            .HasColumnName("deleted_by_id")
            .HasColumnType("uuid")
            .IsRequired(false);
    }
}
