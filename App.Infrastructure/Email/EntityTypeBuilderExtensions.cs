using App.Domain.Common;
using App.Domain.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Email;

public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Configures the owned <see cref="Address"/> value object.
    /// 
    /// All address columns are nullable because the address itself is optional.  
    /// The domain enforces an "all-or-nothing" rule (either null, or a fully 
    /// populated address with Line1/City/State/PostalCode). EF does not enforce
    /// this; completeness is validated in the entity's factory/mutators.
    /// </summary>
    public static void OwnsAddress<T>(this EntityTypeBuilder<T> builder)
        where T : class
    {
        builder.OwnsOne<Address>(
            nameof(Address),
            add =>
            {
                add.Property(a => a.Line1).HasColumnName("line_1").HasMaxLength(64).IsRequired(false);
                add.Property(a => a.Line2).HasColumnName("line_2").HasMaxLength(64).IsRequired(false);
                add.Property(a => a.City).HasColumnName("city").HasMaxLength(50).IsRequired(false);
                add.Property(a => a.State).HasColumnName("state").HasMaxLength(50).IsRequired(false);
                add.Property(a => a.PostalCode).HasColumnName("postal_code").HasMaxLength(15).IsRequired(false);
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
