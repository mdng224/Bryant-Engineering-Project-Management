using App.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Data.Configurations;

public sealed class ClientConfig : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> entity)
    {
        entity.ToTable("clients");

        entity.HasKey(c => c.UserId);

        entity.HasOne(c => c.User)
              .WithOne()
              .HasForeignKey<Client>(c => c.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.Property(c => c.CompanyName).HasMaxLength(200);

        // TODO: Finalize this and add audit fields
    }
}