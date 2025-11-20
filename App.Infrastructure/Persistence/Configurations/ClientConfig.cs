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
                sql: " btrim(coalesce(name, ''))       <> '' ");
        });

        // --- Key ------------------------------------------------------------
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedNever();

        // --- Columns: identity / person ------------------------------------
        b.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        b.Property(c => c.ProjectCode).HasColumnName("project_code").HasMaxLength(32);

        // --- Classification (nullable to allow imperfect CSV matches) -------
        b.Property(c => c.CategoryId).HasColumnName("client_category_id");
        b.Property(c => c.TypeId).HasColumnName("client_type_id");

        // --- Auditing / soft delete ----------------------------------------
        b.ConfigureAuditable();
        b.ConfigureSoftDeletable();
        b.HasQueryFilter(c => c.DeletedAtUtc == null);

        // --- Indexes --------------------------------------------------------
        b.HasIndex(c => c.Name);
        b.HasIndex(c => c.ProjectCode);
        b.HasIndex(c => new { ClientCategoryId = c.CategoryId, ClientTypeId = c.TypeId });
    }
}
