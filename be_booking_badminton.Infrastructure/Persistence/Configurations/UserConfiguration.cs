using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        EntityConfiguration.Configure(builder, "users");

        builder.Property(entity => entity.Email)
            .HasColumnName("email")
            .HasColumnType("varchar(320)")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(entity => entity.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("varchar(512)")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .HasConversion(new EnumStorageConverter<UserStatus>())
            .IsRequired()
            .HasDefaultValue(UserStatus.PendingVerification)
            .HasSentinel(UserStatus.PendingVerification);

        builder.Property(entity => entity.EmailVerifiedAt)
            .HasColumnName("email_verified_at")
            .HasColumnType("timestamptz");

        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint("ck_users_status",
                "status IN ('PENDING_VERIFICATION', 'ACTIVE', 'SUSPENDED', 'DISABLED')");
            table.HasCheckConstraint("ck_users_email_normalized",
                "email = lower(btrim(email))");
            table.HasCheckConstraint("ck_users_email_format",
                "position('@' IN email) > 1");
        });

        builder.HasIndex(entity => entity.Email, "ux_users_email")
            .HasDatabaseName("ux_users_email")
            .IsUnique();

        builder.HasIndex(entity => entity.Status, "ix_users_status_active")
            .HasDatabaseName("ix_users_status_active")
            .HasFilter("is_deleted = false");
    }
}
