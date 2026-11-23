using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.identity;

public sealed class UserProfileConfiguration : BaseEntityConfiguration<UserProfile>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles", table =>
        {
            table.HasCheckConstraint("ck_user_profiles_preferred_locale", "preferred_locale IN ('vi', 'en')");

            // Username có unique => cũng phải chuẩn hóa chữ thường
            table.HasCheckConstraint(
                "ck_user_profiles_username_normalized",
                "username IS NULL OR username = lower(btrim(username))");
        });

        builder.Property(x => x.Username).HasMaxLength(50);
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048);
        builder.Property(x => x.Bio).HasMaxLength(500);
        builder.Property(x => x.PreferredLocale).IsRequired().HasMaxLength(10).HasDefaultValue("vi");

        // 1 User - 1 Profile
        builder.HasNamedIndex(x => x.UserId, "ux_user_profiles_user_id").IsUnique();

        // Username duy nhất (chỉ xét khi có username và chưa xóa)
        builder.HasNamedIndex(x => x.Username, "ux_user_profiles_username_active")
            .UniqueWhenActive("username IS NOT NULL");

        builder.HasOne(x => x.User).WithOne(user => user.Profile).HasForeignKey<UserProfile>(x => x.UserId);
    }
}
