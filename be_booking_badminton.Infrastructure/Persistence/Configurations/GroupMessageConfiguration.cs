using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class GroupMessageConfiguration : IEntityTypeConfiguration<GroupMessage>
{
    public void Configure(EntityTypeBuilder<GroupMessage> builder)
    {
        builder.ToTable("GroupMessages", table =>
        {
            // Sequence của tin nhắn bắt đầu từ 1 trong từng nhóm.
            table.HasCheckConstraint("CKGroupMessageSequence", "\"Sequence\" >= 1");
            // Tin chưa thu hồi phải có nội dung 1–2000 ký tự; tin đã thu hồi phải xóa nội dung và RevokedAt không được trước CreatedAt.
            table.HasCheckConstraint("CKGroupMessageContent", "(\"RevokedAt\" IS NULL AND \"Content\" IS NOT NULL AND length(btrim(\"Content\")) BETWEEN 1 AND 2000) OR (\"RevokedAt\" IS NOT NULL AND \"Content\" IS NULL AND \"RevokedAt\" >= \"CreatedAt\")");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(message => message.Content).HasMaxLength(2000);
        builder.Property(message => message.RevokedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(message => message.GroupConversation)
            .WithMany(group => group.Messages)
            .HasForeignKey(message => message.GroupConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(message => message.SenderUser)
            .WithMany()
            .HasForeignKey(message => message.SenderUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(message => new { message.SenderUserId, message.ClientMessageId })
            .IsUnique()
            .HasDatabaseName("UXGroupMessageSenderClientId");
        builder.HasIndex(message => new { message.GroupConversationId, message.Sequence })
            .IsUnique()
            .IsDescending(false, true)
            .HasDatabaseName("UXGroupMessageGroupSequence");
    }
}
