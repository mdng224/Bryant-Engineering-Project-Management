using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        // --- Table & Key  -----------------------------------------------------------------
        b.ToTable("employees", t =>
        {
            t.HasCheckConstraint(
                "ck_employee_employment_type_valid",
                "\"EmploymentType\" IS NULL OR \"EmploymentType\" IN ('FullTime','PartTime')"
            );
            t.HasCheckConstraint(
                "ck_employee_salary_type_valid",
                "\"SalaryType\" IS NULL OR \"SalaryType\" IN ('Salary','Hourly')"
            );
        });
        b.HasKey(e => e.Id);

        // --- Properties -----------------------------------------------------
        b.Property(e => e.Id).ValueGeneratedNever();    // Guid v7 set in domain
        b.Property(e => e.CompanyEmail).HasMaxLength(128);
        b.Property(e => e.WorkLocation).HasMaxLength(200);
        b.Property(e => e.LicenseNotes).HasMaxLength(200);
        b.Property(e => e.Notes).HasMaxLength(500);
        b.Property(e => e.EmploymentType).HasConversion<string>().HasColumnType("text");
        b.Property(e => e.SalaryType).HasConversion<string>().HasColumnType("text");
        b.Property(u => u.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(u => u.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamptz");
        b.Property(u => u.DeletedAtUtc).HasColumnType("timestamptz");
        
        // --- Relationships --------------------------------------------------
        b.HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // NOTE: positions are managed via EmployeePosition.
        
        // --- Indexes / Uniqueness ------------------------------------------
        b.HasIndex(e => e.UserId)
            .IsUnique()
            .HasFilter("\"UserId\" IS NOT NULL AND \"DeletedAtUtc\" IS NULL");
        b.HasIndex(e => e.CompanyEmail)
            .HasDatabaseName("ux_employees_company_email")
            .IsUnique()
            .HasFilter("\"CompanyEmail\" IS NOT NULL AND \"DeletedAtUtc\" IS NULL");
        
        // TODO: Seed employees
    }
}