using App.Domain.Projects;
using App.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        // --- Table ----------------------------------------------------------
        b.ToTable("projects");

        // --- Key ------------------------------------------------------------
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedNever(); // Guid v7 provided in code

        // --- Columns --------------------------------------------------------
        b.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        b.Property(p => p.Code).HasColumnName("code").HasMaxLength(7).IsRequired();
        b.Property(p => p.Year).HasColumnName("year").IsRequired();
        b.Property(p => p.Number).HasColumnName("number").IsRequired();
        // Computed property - do NOT persist as a regular column
        b.Ignore(p => p.NewCode);
        b.Property(p => p.Scope).HasColumnName("scope").HasMaxLength(100).IsRequired();
        b.Property(p => p.Manager).HasColumnName("manager").HasMaxLength(100).IsRequired();
        b.Property(p => p.Type).HasColumnName("type").HasMaxLength(64).IsRequired();
        b.OwnsAddress();
        b.ConfigureAuditable();

        // FK to Client -------------------------------------------------------
        b.Property(p => p.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        b.HasOne(p => p.Client)
            .WithMany(p => p.Projects)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Soft-delete filter -------------------------------------------------
        b.HasQueryFilter(p => p.DeletedAtUtc == null);
        
        // Indexes / uniqueness ----------------------------------------------
        // If project codes are globally unique:
        b.HasIndex(p => p.Code).IsUnique();
    }
}