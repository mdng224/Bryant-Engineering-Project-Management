using App.Domain.Contacts;
using App.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class ContactConfig : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> b)
    {
        b.ToTable("contacts", tb =>
        {
            tb.HasCheckConstraint(
                name: "ck_contacts_name_required",
                sql: "btrim(coalesce(first_name, '')) <> '' AND btrim(coalesce(last_name, '')) <> ''");
        });
        
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedNever();
        
        // --- Person name ----------------------------------------------------
        b.Property(c => c.NamePrefix).HasColumnName("name_prefix").HasMaxLength(32);
        b.Property(c => c.FirstName).HasColumnName("first_name").HasMaxLength(128).IsRequired();
        b.Property(c => c.MiddleName).HasColumnName("middle_name").HasMaxLength(128);
        b.Property(c => c.LastName).HasColumnName("last_name").HasMaxLength(128).IsRequired();
        b.Property(c => c.NameSuffix).HasColumnName("name_suffix").HasMaxLength(32);
        b.Property(c => c.Company).HasColumnName("company").HasMaxLength(256);
        b.Property(c => c.Department).HasColumnName("department").HasMaxLength(128);
        b.Property(c => c.JobTitle).HasColumnName("job_title").HasMaxLength(256);
        
        b.OwnsAddress();
        b.Property(c => c.Country).HasColumnName("country").HasMaxLength(128);
        
        // --- Phones ---------------------------------------------------------
        b.Property(c => c.BusinessPhone).HasColumnName("business_phone").HasMaxLength(64);
        b.Property(c => c.MobilePhone).HasColumnName("mobile_phone").HasMaxLength(64);
        b.Property(c => c.PrimaryPhone).HasColumnName("primary_phone").HasConversion<string>().HasMaxLength(32);
        
        // --- Email / web ----------------------------------------------------
        b.Property(c => c.Email).HasColumnName("email").HasMaxLength(256);
        b.Property(c => c.WebPage).HasColumnName("web_page").HasMaxLength(512);
        
        // --- Primary flag ---------------------------------------------------
        b.Property(c => c.IsPrimaryForClient)
            .HasColumnName("is_primary_for_client");

        // --- Auditing / soft delete ----------------------------------------
        b.ConfigureAuditable();
        b.ConfigureSoftDeletable();
        b.HasQueryFilter(c => c.DeletedAtUtc == null);

        // --- Indexes --------------------------------------------------------
        b.HasIndex(c => c.ClientId);
        b.HasIndex(c => new { c.LastName, c.FirstName });
        b.HasIndex(c => c.Email);
        b.HasIndex(c => c.Company);
        
        // --- Foreign keys ---------------------------------------------------
        b.HasOne(c => c.Client)
            .WithMany() // if you later add Client.Contacts, change to .WithMany(c => c.Contacts)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}