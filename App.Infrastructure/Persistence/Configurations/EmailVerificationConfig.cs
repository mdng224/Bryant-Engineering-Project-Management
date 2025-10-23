using App.Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class EmailVerificationConfig : IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> b)
    {
        // --- Table ----------------------------------------------------------
        b.ToTable("email_verifications", t =>
        {
            t.HasCheckConstraint(
                name: "ck_email_verifications_expiry_future",
                sql: "\"expires_at_utc\" > NOW() - INTERVAL '7 days'"
            );
        });

        // --- Key ------------------------------------------------------------
        b.HasKey(ev => ev.Id);

        // --- Properties -----------------------------------------------------
        b.Property(ev => ev.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(ev => ev.UserId).HasColumnName("user_id").IsRequired();
        b.Property(ev => ev.TokenHash).HasColumnName("token_hash").HasColumnType("text").IsRequired();
        b.Property(ev => ev.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();
        b.Property(ev => ev.Used).HasColumnName("used").HasDefaultValue(false).IsRequired();

        // --- Indexes --------------------------------------------------------
        b.HasIndex(ev => ev.UserId).HasDatabaseName("ix_email_verifications_user_id");
        b.HasIndex(ev => ev.TokenHash).IsUnique().HasDatabaseName("ux_email_verifications_token_hash");
        b.HasIndex(ev => ev.ExpiresAtUtc).HasDatabaseName("ix_email_verifications_expires_at");
    }
}