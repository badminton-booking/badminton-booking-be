using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles", table =>
        {
            // Username, nếu có, phải là chữ thường và không có khoảng trắng đầu/cuối.
            table.HasCheckConstraint("CKUserProfileUsernameNormalized", "\"Username\" IS NULL OR \"Username\" = lower(btrim(\"Username\"))");
            // Username, nếu có, tối thiểu 3 ký tự.
            table.HasCheckConstraint("CKUserProfileUsernameNotBlank", "\"Username\" IS NULL OR length(\"Username\") >= 3");
            // Họ tên không được rỗng.
            table.HasCheckConstraint("CKUserProfileFullNameNotBlank", "length(btrim(\"FullName\")) > 0");
            // Số điện thoại, nếu có, không được rỗng.
            table.HasCheckConstraint("CKUserProfilePhoneNotBlank", "\"PhoneNumber\" IS NULL OR length(btrim(\"PhoneNumber\")) > 0");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(profile => profile.Username).HasMaxLength(50);
        builder.Property(profile => profile.FullName).HasMaxLength(150).IsRequired();
        builder.Property(profile => profile.PhoneNumber).HasMaxLength(20);
        builder.Property(profile => profile.AvatarUrl).HasMaxLength(2048);
        builder.Property(profile => profile.Bio).HasMaxLength(500);

        builder.HasOne(profile => profile.User)
            .WithOne(user => user.Profile)
            .HasForeignKey<UserProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(profile => profile.UserId).IsUnique().HasDatabaseName("UXUserProfileUserId");
        builder.HasIndex(profile => profile.Username)
            .IsUnique()
            .HasFilter("\"Username\" IS NOT NULL AND \"IsDeleted\" = false")
            .HasDatabaseName("UXUserProfileUsernameActive");
        builder.HasIndex(profile => profile.PhoneNumber)
            .HasFilter("\"PhoneNumber\" IS NOT NULL AND \"IsDeleted\" = false")
            .HasDatabaseName("IXUserProfilePhoneNumberActive");
    }
}
