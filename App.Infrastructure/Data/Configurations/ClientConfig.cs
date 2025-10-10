using App.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Data.Configurations;

public sealed class ClientConfig : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> entity)
    {
        entity.ToTable("clients");

        // Key
        entity.HasKey(c => c.Id);
        entity.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 provided in code

        // Core fields
        entity.Property(c => c.CompanyName)
              .HasMaxLength(200)        // optional
              .IsRequired(false);

        entity.Property(c => c.ContactName)
              .HasMaxLength(200)        // optional
              .IsRequired(false);

        entity.Property(c => c.Email)
              .IsRequired()
              .HasMaxLength(320);       // RFC upper bound

        // Case-insensitive uniqueness for active (non-deleted) clients
        entity.Property<string>("EmailNormalized")
              .HasColumnType("text")
              .HasComputedColumnSql("lower(\"Email\")", stored: true);

        entity.HasIndex("EmailNormalized")
              .IsUnique()
              .HasDatabaseName("ux_clients_email_normalized")
              .HasFilter("\"DeletedAtUtc\" IS NULL"); // Postgres filtered unique

        entity.Property(c => c.Phone).HasMaxLength(32);

        entity.Property(c => c.AddressLine1).HasMaxLength(200);
        entity.Property(c => c.AddressLine2).HasMaxLength(200);
        entity.Property(c => c.City).HasMaxLength(100);
        entity.Property(c => c.StateOrProvince).HasMaxLength(100);
        entity.Property(c => c.PostalCode).HasMaxLength(32);
        entity.Property(c => c.Country).HasMaxLength(100);

        entity.Property(c => c.Note).HasColumnType("text"); // free-form

        // Auditing / soft delete
        entity.Property(c => c.CreatedAtUtc)
              .IsRequired()
              .HasColumnType("timestamptz")
              .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.Property(c => c.UpdatedAtUtc)
              .IsRequired()
              .HasColumnType("timestamptz")
              .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.Property(c => c.DeletedAtUtc)
              .HasColumnType("timestamptz");

        // Hide soft-deleted rows by default
        entity.HasQueryFilter(c => c.DeletedAtUtc == null);

        // Optional (Postgres): optimistic concurrency
        // entity.UseXminAsConcurrencyToken();
    }
}