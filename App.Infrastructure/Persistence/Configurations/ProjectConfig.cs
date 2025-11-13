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
        b.Property(p => p.Name)     .HasColumnName("name").HasMaxLength(200).IsRequired();
        b.Property(p => p.Code)     .HasColumnName("code").HasMaxLength(32).IsRequired();
        b.Property(p => p.Year)     .HasColumnName("year").IsRequired();
        b.Property(p => p.Number)   .HasColumnName("number").IsRequired();
        b.Ignore (p => p.NewCode); // computed in code
        b.Property(p => p.Manager)  .HasColumnName("manager").HasMaxLength(100).IsRequired();
        b.Property(p => p.Type)     .HasColumnName("type").HasMaxLength(64).IsRequired();
        b.Property(p => p.Location) .HasColumnName("location").HasMaxLength(200).IsRequired();

        // --- FKs ------------------------------------------------------------
        // Client
        b.Property(p => p.ClientId).HasColumnName("client_id").IsRequired();
        b.HasOne(p => p.Client)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Scope (1 project -> 1 scope)
        b.Property(p => p.ScopeId).HasColumnName("scope_id").IsRequired();
        b.HasOne(p => p.Scope)
            .WithMany()
            .HasForeignKey(p => p.ScopeId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Auditing / soft delete ----------------------------------------
        b.ConfigureAuditable();
        b.HasQueryFilter(p => p.DeletedAtUtc == null);

        // --- Indexes / uniqueness ------------------------------------------
        b.HasIndex(p => p.Code).IsUnique();
        b.HasIndex(p => new { p.Year, p.Number }).IsUnique(); // if guaranteed unique
    }
}