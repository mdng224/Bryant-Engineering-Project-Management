using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Data.Configurations;

public sealed class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> entity)
    {
        entity.ToTable("employees");

        entity.HasKey(e => e.UserId);

        entity.HasOne(e => e.User)
              .WithOne() // keep User clean; or expose User.Employee nav later
              .HasForeignKey<Employee>(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.Property(e => e.JobTitle).HasMaxLength(200);

        // TODO: Finalize this and add audit fields
    }
}