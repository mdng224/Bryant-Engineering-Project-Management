using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class EmployeePositionConfig : IEntityTypeConfiguration<EmployeePosition>
{
    public void Configure(EntityTypeBuilder<EmployeePosition> b)
    {
        // --- Table ----------------------------------------------------------
        b.ToTable("employee_positions");

        // --- Keys -----------------------------------------------------------
        b.HasKey(ep => new { ep.EmployeeId, ep.PositionId });

        // --- Columns --------------------------------------------------------
        b.Property(ep => ep.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        b.Property(ep => ep.PositionId)
            .HasColumnName("position_id")
            .IsRequired();

        // --- Relationships --------------------------------------------------
        b.HasOne(ep => ep.Employee)
            .WithMany(e => e.Positions)
            .HasForeignKey(ep => ep.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(ep => ep.Position)
            .WithMany()
            .HasForeignKey(ep => ep.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Indexes --------------------------------------------------------
        b.HasIndex(ep => ep.PositionId)
            .HasDatabaseName("ix_employee_positions_position_id");
        
        b.HasIndex(ep => ep.EmployeeId)
            .HasDatabaseName("ix_employee_positions_employee_id");
    }
}