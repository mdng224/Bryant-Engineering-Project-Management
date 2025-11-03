using App.Domain.Employees;
using App.Infrastructure.Email;
using App.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class PositionConfig : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> b)
    {
        // --- Table & Key ----------------------------------------------------
        b.ToTable("positions");
        b.HasKey(p => p.Id);
        
        // --- Properties -----------------------------------------------------
        b.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        b.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(128);
        
        b.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(16);
        
        b.Property(p => p.RequiresLicense)
            .HasColumnName("requires_license")
            .IsRequired();
        
        b.ConfigureAuditable();
        b.ConfigureSoftDeletable();
        
        // --- Indexes / Uniqueness ------------------------------------------
        b.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("ix_positions_name");
        
        b.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("ix_positions_code");
    }
}