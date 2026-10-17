using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        EntityConfiguration.Configure(builder, "user_profiles");

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Username)
            .HasColumnName("username")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(entity => entity.FullName)
            .HasColumnName("full_name")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(entity => entity.PhoneNumber)
            .HasColumnName("phone_number")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20);

        builder.Property(entity => entity.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasColumnType("varchar(2048)")
            .HasMaxLength(2048);

        builder.Property(entity => entity.Bio)
            .HasColumnName("bio")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500);

        builder.HasOne(entity => entity.User)
            .WithOne()
            .HasForeignKey<UserProfile>(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_user_profiles_user_id");

        builder.ToTable("user_profiles", table =>
        {
            table.HasCheckConstraint("ck_user_profiles_username_normalized",
                "username IS NULL OR username = lower(btrim(username))");
            table.HasCheckConstraint("ck_user_profiles_username_not_blank",
                "username IS NULL OR length(username) >= 3");
            table.HasCheckConstraint("ck_user_profiles_full_name_not_blank",
                "length(btrim(full_name)) > 0");
            table.HasCheckConstraint("ck_user_profiles_phone_not_blank",
                "phone_number IS NULL OR length(btrim(phone_number)) > 0");
        });

        builder.HasIndex(entity => entity.UserId, "ux_user_profiles_user_id")
            .HasDatabaseName("ux_user_profiles_user_id")
            .IsUnique();

        builder.HasIndex(entity => entity.Username, "ux_user_profiles_username_active")
            .HasDatabaseName("ux_user_profiles_username_active")
            .IsUnique()
            .HasFilter("username IS NOT NULL AND is_deleted = false");

        builder.HasIndex(entity => entity.PhoneNumber, "ix_user_profiles_phone_number_active")
            .HasDatabaseName("ix_user_profiles_phone_number_active")
            .HasFilter("phone_number IS NOT NULL AND is_deleted = false");
    }
}
