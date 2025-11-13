using App.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class ClientCategoryConfig : IEntityTypeConfiguration<ClientCategory>
{
    public void Configure(EntityTypeBuilder<ClientCategory> b)
    {
        // --- Table & constraints -------------------------------------------
        b.ToTable("client_categories", tb =>
        {
            tb.HasCheckConstraint(
                name: "ck_client_category_name_not_blank",
                sql: "length(btrim(name)) > 0");
        });
        
        // --- Key ------------------------------------------------------------
        b.HasKey(cc => cc.Id);
        b.Property(cc => cc.Id).ValueGeneratedNever();
        
        // --- Columns --------------------------------------------------------
        b.Property(cc => cc.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        b.HasIndex(cc => cc.Name).IsUnique();
        
        // --- Relationships: Category -> Types (1:N) -------------------------
        b.HasMany(cg => cg.Types)
            .WithOne(ct => ct.Category)
            .HasForeignKey(ct => ct.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}