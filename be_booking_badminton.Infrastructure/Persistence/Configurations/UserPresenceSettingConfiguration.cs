using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class UserPresenceSettingConfiguration : IEntityTypeConfiguration<UserPresenceSetting>
{
    public void Configure(EntityTypeBuilder<UserPresenceSetting> builder)
    {
        builder.ToTable("UserPresenceSettings", table =>
            // Chỉ cho phép các giá trị đã định nghĩa trong PresenceVisibility.
            table.HasCheckConstraint("CKUserPresenceSettingVisibility", "\"Visibility\" IN ('visible', 'hidden')"));
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(setting => setting.Visibility)
            .HasConversion(visibility => visibility.Name, name => PresenceVisibility.FromName(name, false))
            .HasMaxLength(20)
            .HasDefaultValue(PresenceVisibility.Visible)
            .IsRequired();
        builder.HasOne(setting => setting.User)
            .WithMany()
            .HasForeignKey(setting => setting.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(setting => setting.UserId)
            .IsUnique()
            .HasDatabaseName("UXUserPresenceSettingUserId");
    }
}
