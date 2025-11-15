using App.Domain.Clients;
using App.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class ClientConfig : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> b)
    {
        // --- Table & constraints -------------------------------------------
        b.ToTable("clients", tb =>
        {
            tb.HasCheckConstraint(
                name: "ck_clients_all_names_required",
                sql: " btrim(coalesce(name, ''))       <> '' " +
                     " AND btrim(coalesce(first_name, '')) <> '' " +
                     " AND btrim(coalesce(last_name, '')) <> '' ");
        });

        // --- Key ------------------------------------------------------------
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedNever();

        // --- Columns: identity / person ------------------------------------
        b.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        b.Property(c => c.NamePrefix).HasColumnName("name_prefix").HasMaxLength(32);
        b.Property(c => c.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        b.Property(c => c.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        b.Property(c => c.NameSuffix).HasColumnName("name_suffix").HasMaxLength(32);

        // --- Columns: contact ----------------------------------------------
        b.Property(c => c.Email).HasColumnName("email").HasMaxLength(254);
        b.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(32);

        // --- Columns: misc --------------------------------------------------
        b.Property(c => c.Note).HasColumnName("note").HasColumnType("text");
        b.Property(c => c.ProjectCode).HasColumnName("project_code").HasMaxLength(32);

        // --- Classification (nullable to allow imperfect CSV matches) -------
        b.Property(c => c.ClientCategoryId).HasColumnName("client_category_id");
        b.Property(c => c.ClientTypeId).HasColumnName("client_type_id");

        // --- Owned types ----------------------------------------------------
        b.OwnsAddress();

        // --- Auditing / soft delete ----------------------------------------
        b.ConfigureAuditable();
        b.ConfigureSoftDeletable();
        b.HasQueryFilter(c => c.DeletedAtUtc == null);

        // --- Indexes --------------------------------------------------------
        b.HasIndex(c => c.Name);
        b.HasIndex(c => new { c.LastName, c.FirstName });
        b.HasIndex(c => c.Email)
            .IsUnique()
            .HasDatabaseName("ux_clients_email_active")
            .HasFilter("email IS NOT NULL AND deleted_at_utc IS NULL");
        b.HasIndex(c => c.ProjectCode);
        b.HasIndex(c => new { c.ClientCategoryId, c.ClientTypeId });
    }
}
