using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.platform;

public sealed class NotificationConfiguration : BaseEntityConfiguration<Notification>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Data).HasColumnType("jsonb");

        // Danh sách thông báo của 1 user (mới nhất trước)
        builder.HasNamedIndex(x => new { x.UserId, x.CreatedAt, x.Id }, "ix_notifications_user_created_at")
            .IsDescending(false, true, false)
            .WhenActive();

        // Đếm / lấy thông báo chưa đọc (chấm đỏ trên chuông)
        builder.HasNamedIndex(x => new { x.UserId, x.CreatedAt }, "ix_notifications_user_unread")
            .IsDescending(false, true)
            .WhenActive("read_at IS NULL");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}
