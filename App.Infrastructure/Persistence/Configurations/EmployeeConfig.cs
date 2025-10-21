using App.Domain.Employees;
using App.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        // --- Table & Key ----------------------------------------------------
        b.ToTable("employees", t =>
        {
            t.HasCheckConstraint(
                "ck_employees_employment_type_valid",
                "employee_type IS NULL OR employee_type IN ('FullTime','PartTime')"
            );
            t.HasCheckConstraint(
                "ck_employees_salary_type_valid",
                "salary_type IS NULL OR salary_type IN ('Salary','Hourly')"
            );
            // hire_date <= end_date when both present
            t.HasCheckConstraint(
                "ck_employees_dates_order",
                "end_date IS NULL OR hire_date IS NULL OR hire_date <= end_date"
            );
            // non-empty names (after trimming)
            t.HasCheckConstraint("ck_employees_first_not_empty", "length(trim(first_name)) > 0");
            t.HasCheckConstraint("ck_employees_last_not_empty",  "length(trim(last_name))  > 0");
        });
        
        b.HasKey(e => e.Id);

        // --- Properties -----------------------------------------------------
        b.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // Guid v7 set in domain
        
        b.Property(e => e.UserId)
            .HasColumnName("user_id"); // FK column
        
        b.Property(e => e.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(100);
        
        b.Property(e => e.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(100);
        
        b.Property(e => e.PreferredName)
            .HasColumnName("preferred_name")
            .HasMaxLength(100);
        
        b.Property(e => e.CompanyEmail)
            .HasColumnName("company_email")
            .HasMaxLength(128);
        
        b.Property(e => e.WorkLocation)
            .HasColumnName("work_location")
            .HasMaxLength(200);
        
        b.Property(e => e.LicenseNotes)
            .HasColumnName("license_notes")
            .HasMaxLength(200);
        
        b.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500);
        
        b.Property(e => e.EmploymentType)
            .HasColumnName("employee_type")
            .HasConversion<string>()
            .HasColumnType("text");
        
        b.Property(e => e.SalaryType)
            .HasColumnName("salary_type")
            .HasConversion<string>()
            .HasColumnType("text");
        
        b.Property(e => e.HireDate)
            .HasColumnName("hire_date")
            .HasColumnType("timestamptz");
        
        b.Property(e => e.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("timestamptz"); // or "date"
        
        b.Property(e => e.Department)
            .HasColumnName("department");
        
        b.Property(u => u.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        b.Property(u => u.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired()
            .HasColumnType("timestamptz");
        
        b.Property(u => u.DeletedAtUtc)
            .HasColumnName("deleted_at_utc")
            .HasColumnType("timestamptz");
        
        // --- Relationships --------------------------------------------------
        b.HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        // --- Indexes / Uniqueness ------------------------------------------
        b.HasIndex(e => e.UserId)
            .IsUnique()
            .HasFilter("user_id IS NOT NULL AND deleted_at_utc IS NULL")
            .HasDatabaseName("ux_employees_user_id");
        
        b.HasIndex(e => e.CompanyEmail)
            .IsUnique()
            .HasFilter("company_email IS NOT NULL AND deleted_at_utc IS NULL")
            .HasDatabaseName("ux_employees_company_email");

        b.HasIndex(e => new { e.LastName, e.FirstName })
            .HasDatabaseName("ix_employees_last_first")
            .HasFilter("deleted_at_utc IS NULL");
    }
}