using App.Domain.Employees;
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
        b.Property(p => p.Id).ValueGeneratedNever();
        b.Property(p => p.Name).IsRequired().HasMaxLength(128);
        b.Property(p => p.Code).HasMaxLength(16);
        b.Property(p => p.RequiresLicense).IsRequired();    // professional license
        
        // --- Indexes / Uniqueness ------------------------------------------
        b.HasIndex(p => p.Name).IsUnique();
        b.HasIndex(p => p.Code).IsUnique();
        
        // --- Seed Data ------------------------------------------------------
        b.HasData(
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000000"), Name = "Principal-In-Charge",        Code = "PIC",           RequiresLicense = true  },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000001"), Name = "Project Engineer",           Code = "PE",            RequiresLicense = true  },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000002"), Name = "Professional Land Surveyor", Code = "PLS",           RequiresLicense = true },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000003"), Name = "Land Surveyor In Training",  Code = "LSIT",          RequiresLicense = true },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000004"), Name = "Survey Party Chief",         Code = "SPC",           RequiresLicense = false },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000005"), Name = "Engineering Intern",         Code = "Eng Intern",    RequiresLicense = false },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000006"), Name = "Rodman",                     Code = "Rodman",        RequiresLicense = false },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000007"), Name = "Office Manager",             Code = "OfficeMgr",     RequiresLicense = false },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000008"), Name = "Drafting Technician",        Code = "Draft Tech",    RequiresLicense = false },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000009"), Name = "Engineering Technician",     Code = "Eng Tech",      RequiresLicense = false },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000010"), Name = "Senior Drafting Technician", Code = "Sr Draft Tech", RequiresLicense = false },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000011"), Name = "Engineer-In-Training",       Code = "EIT",           RequiresLicense = false },
            new { Id = Guid.Parse("b2d8f9a4-8e8a-4b8a-8b30-000000000012"), Name = "Remote Pilot In Command",    Code = "Remote PIC",    RequiresLicense = false }
        );
    }
}