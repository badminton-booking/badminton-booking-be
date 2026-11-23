using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.identity;

public sealed class UserConfiguration : BaseEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table =>
        {
            // Unique index phân biệt hoa/thường => email BẮT BUỘC đã được chuẩn hóa chữ thường
            table.HasCheckConstraint("ck_users_email_normalized", "email = lower(btrim(email))");
        });

        builder.Property(x => x.Email).IsRequired().HasMaxLength(320);
        builder.Property(x => x.PasswordHash).HasMaxLength(512);
        builder.Property(x => x.BanReason).HasMaxLength(1000);

        // Email là duy nhất trên toàn hệ thống (kể cả tài khoản đã xóa mềm).
        builder.HasNamedIndex(x => x.Email, "ux_users_email").IsUnique();
    }
}
