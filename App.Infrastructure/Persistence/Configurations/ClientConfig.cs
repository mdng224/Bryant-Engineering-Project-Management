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
                " OR (btrim(coalesce(contact_first,  '')) <> '') " +
                " OR (btrim(coalesce(contact_last,   '')) <> '') ");
        });

        // --- Key ------------------------------------------------------------
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 provided in code

        // --- Columns --------------------------------------------------------
        b.Property(c => c.Name).HasColumnName("name").HasMaxLength(200).IsRequired(false);
        b.Property(c => c.ContactFirst).HasColumnName("contact_first").HasMaxLength(100).IsRequired(false);
        b.Property(c => c.ContactMiddle).HasColumnName("contact_middle").HasMaxLength(100).IsRequired(false);
        b.Property(c => c.ContactLast).HasColumnName("contact_last").HasMaxLength(100).IsRequired(false);
        b.Property(c => c.Email).HasColumnName("email").HasMaxLength(128).IsRequired(false); // email is optional
        b.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(32).IsRequired(false);
        b.Property(c => c.Note).HasColumnName("note").HasColumnType("text").IsRequired(false);
        b.Property(c => c.ProjectCode).HasColumnName("project_code").HasMaxLength(7).IsRequired(false);

        b.ConfigureAuditable();

        // --- Relationships --------------------------------------------------
        b.OwnsAddress();

        b.HasQueryFilter(c => c.DeletedAtUtc == null);
    }
}