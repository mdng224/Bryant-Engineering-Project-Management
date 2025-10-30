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
        b.ToTable("clients");

        // --- Key ------------------------------------------------------------
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 provided in code

        // --- Columns --------------------------------------------------------
        b.Property(c => c.CompanyName).HasColumnName("company_name").HasMaxLength(200).IsRequired(false);
        b.Property(c => c.ContactName).HasColumnName("contact_name").HasMaxLength(200).IsRequired(false);
        b.Property(e => e.Email).HasColumnName("email").HasMaxLength(128);
        b.Property(c => c.Note).HasColumnType("text");
        b.ConfigureAuditable();

        // --- Relationships --------------------------------------------------
        b.OwnsAddress();
    }
}