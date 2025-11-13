using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

internal sealed class ScopeConfig : IEntityTypeConfiguration<Scope>
{
    public void Configure(EntityTypeBuilder<Scope> b)
    {
        b.ToTable("scopes");
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).ValueGeneratedNever();
        
        b.Property(s => s.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        b.Property(s => s.Description).HasMaxLength(512);
        b.HasIndex(s => s.Name).IsUnique(); // each scope name unique
    }
}