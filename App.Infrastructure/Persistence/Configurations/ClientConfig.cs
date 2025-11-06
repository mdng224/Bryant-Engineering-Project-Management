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
                sql: " (btrim(coalesce(company_name, '')) <> '') " +
                " OR (btrim(coalesce(first_name,  '')) <> '') " +
                " OR (btrim(coalesce(last_name,   '')) <> '') ");
        });

        // --- Key ------------------------------------------------------------
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 provided in code

        // --- Columns --------------------------------------------------------
        b.Property(c => c.Name).HasColumnName("company_name").HasMaxLength(200).IsRequired(false);
        b.Property(c => c.ContactFirst).HasColumnName("first_name").HasMaxLength(100).IsRequired(false);
        b.Property(c => c.ContactMiddle).HasColumnName("middle_name").HasMaxLength(100).IsRequired(false);
        b.Property(c => c.ContactLast).HasColumnName("last_name").HasMaxLength(100).IsRequired(false);
        b.Property(c => c.Email).HasColumnName("email").HasMaxLength(128).IsRequired(false); // email is optional
        b.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(32).IsRequired(false);
        b.Property(c => c.Note).HasColumnName("note").HasColumnType("text").IsRequired(false);

        b.ConfigureAuditable();

        // --- Relationships --------------------------------------------------
        b.OwnsAddress();
        // TODO: Add project here        
        b.HasQueryFilter(c => c.DeletedAtUtc == null);
    }
}