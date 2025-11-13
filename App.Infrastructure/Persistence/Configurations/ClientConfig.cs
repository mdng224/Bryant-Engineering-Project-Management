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
                name: "ck_clients_company_or_person",
                sql: " (btrim(coalesce(name, '')) <> '') " +
                " OR (btrim(coalesce(first_name,  '')) <> '') " +
                " OR (btrim(coalesce(last_name,   '')) <> '') ");
        });

        // --- Key ------------------------------------------------------------
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 provided in code

        // --- Columns: identity / person ------------------------------------
        b.Property(c => c.Name)       .HasColumnName("name")        .HasMaxLength(200).IsRequired(false);
        b.Property(c => c.NamePrefix) .HasColumnName("name_prefix") .HasMaxLength(32) .IsRequired(false);
        b.Property(c => c.FirstName)  .HasColumnName("first_name")  .HasMaxLength(100).IsRequired(false);
        b.Property(c => c.LastName)   .HasColumnName("last_name")   .HasMaxLength(100).IsRequired(false);
        b.Property(c => c.NameSuffix) .HasColumnName("name_suffix") .HasMaxLength(32) .IsRequired(false);
        
        // --- Columns: contact ----------------------------------------------
        b.Property(c => c.Email)      .HasColumnName("email")       .HasMaxLength(254).IsRequired(false);
        b.Property(c => c.Phone)      .HasColumnName("phone")       .HasMaxLength(32) .IsRequired(false);

        // --- Columns: misc --------------------------------------------------
        b.Property(c => c.Note)       .HasColumnName("note").HasColumnType("text").IsRequired(false);
        b.Property(c => c.ProjectCode).HasColumnName("project_code").HasMaxLength(32).IsRequired(false);

        // --- Classification (nullable to allow imperfect CSV matches) -------
        b.Property(c => c.ClientCategoryId).HasColumnName("client_category_id").IsRequired(false);
        b.Property(c => c.ClientTypeId)    .HasColumnName("client_type_id")    .IsRequired(false);
        
        // --- Owned types ----------------------------------------------------
        b.OwnsAddress();

        // --- Auditing / soft delete ----------------------------------------
        b.ConfigureAuditable();
        b.HasQueryFilter(c => c.DeletedAtUtc == null);

        // --- Indexes --------------------------------------------------------
        // Useful lookups; adjust as needed
        b.HasIndex(c => c.Name);
        b.HasIndex(c => new { c.LastName, c.FirstName });
        b.HasIndex(c => c.Email);
        b.HasIndex(c => c.ProjectCode);
        b.HasIndex(c => new { c.ClientCategoryId, c.ClientTypeId });
    }
}