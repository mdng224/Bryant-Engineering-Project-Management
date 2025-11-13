using App.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class ClientTypeConfig : IEntityTypeConfiguration<ClientType>
{
    public void Configure(EntityTypeBuilder<ClientType> b)
    {
        // --- Table & constraints -------------------------------------------
        b.ToTable("client_types", tb =>
        {
            tb.HasCheckConstraint(
                name: "ck_client_type_name_not_blank",
                sql: "length(btrim(name)) > 0");
        });
        
        // --- Key ------------------------------------------------------------
        b.HasKey(ct => ct.Id);
        b.Property(ct => ct.Id).ValueGeneratedNever();
        
        // --- Columns --------------------------------------------------------
        b.Property(ct => ct.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        b.HasIndex(ct => ct.Name).IsUnique();
        b.Property(ct => ct.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(500);

        // Prevent duplicate type names within the same group
        b.HasIndex(ct => new { ct.CategoryId, ct.Name }).IsUnique();
        
        // Required FK to Group
        b.HasOne(ct => ct.Category)
            .WithMany(ctg => ctg.Types)
            .HasForeignKey(ct => ct.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}