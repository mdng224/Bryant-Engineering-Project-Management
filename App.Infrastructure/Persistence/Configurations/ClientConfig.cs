using App.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class ClientConfig : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> b)
    {
        b.ToTable("clients");

        // Key
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 provided in code

        // Core fields
        b.Property(c => c.CompanyName)
              .HasMaxLength(200)        // optional
              .IsRequired(false);

        b.Property(c => c.ContactName)
              .HasMaxLength(200)        // optional
              .IsRequired(false);

        b.Property(c => c.Email)
              .IsRequired()
              .HasMaxLength(320);       // RFC upper bound

        // Case-insensitive uniqueness for active (non-deleted) clients
        b.Property<string>("EmailNormalized")
              .HasColumnType("text")
              .HasComputedColumnSql("lower(\"Email\")", stored: true);

        b.HasIndex("EmailNormalized")
              .IsUnique()
              .HasDatabaseName("ux_clients_email_normalized")
              .HasFilter("\"DeletedAtUtc\" IS NULL"); // Postgres filtered unique

        b.Property(c => c.Phone)
              .HasMaxLength(15);
        b.Property(c => c.AddressLine1)
              .HasMaxLength(100);
        b.Property(c => c.AddressLine2)
              .HasMaxLength(50);
        b.Property(c => c.City)
              .HasMaxLength(60);
        b.Property(c => c.StateOrProvince)
              .HasMaxLength(2);     // US state abbreviation
        b.Property(c => c.PostalCode)
              .HasMaxLength(10);
        b.Property(c => c.Country)
              .HasMaxLength(2);     // ISO Alpha-2 code

        b.Property(c => c.Note)
              .HasColumnType("text");

        // Auditing / soft delete
        b.Property(c => c.CreatedAtUtc)
              .IsRequired()
              .HasColumnType("timestamptz")
              .HasDefaultValueSql("CURRENT_TIMESTAMP");

        b.Property(c => c.UpdatedAtUtc)
              .IsRequired()
              .HasColumnType("timestamptz")
              .HasDefaultValueSql("CURRENT_TIMESTAMP");

        b.Property(c => c.DeletedAtUtc)
              .HasColumnType("timestamptz");

        // Hide soft-deleted rows by default
        b.HasQueryFilter(c => c.DeletedAtUtc == null);

        // Optional (Postgres): optimistic concurrency
        // b.UseXminAsConcurrencyToken();
    }
}