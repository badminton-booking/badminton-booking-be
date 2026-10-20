using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", table =>
        {
            // Chỉ cho phép các trạng thái đã định nghĩa trong UserStatus.
            table.HasCheckConstraint("CKUserStatus", "\"Status\" IN ('pending_verification', 'active', 'suspended', 'disabled')");
            // Email phải được chuẩn hóa: chữ thường và không có khoảng trắng đầu/cuối.
            table.HasCheckConstraint("CKUserEmailNormalized", "\"Email\" = lower(btrim(\"Email\"))");
            // Email phải có ký tự '@' và phần trước '@' không rỗng.
            table.HasCheckConstraint("CKUserEmailFormat", "position('@' IN \"Email\") > 1");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(user => user.Email).HasMaxLength(320).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(user => user.Status)
            .HasConversion(status => status.Name, name => UserStatus.FromName(name, false))
            .HasMaxLength(30)
            .HasDefaultValue(UserStatus.PendingVerification)
            .IsRequired();
        builder.Property(user => user.EmailVerifiedAt).HasColumnType("timestamp with time zone");

        builder.HasIndex(user => user.Email).IsUnique().HasDatabaseName("UXUserEmail");
        builder.HasIndex(user => user.Status)
            .HasDatabaseName("IXUserStatusActive")
            .HasFilter("\"IsDeleted\" = false");
    }
}
