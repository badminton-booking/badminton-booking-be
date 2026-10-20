using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class DirectMessageConfiguration : IEntityTypeConfiguration<DirectMessage>
{
    public void Configure(EntityTypeBuilder<DirectMessage> builder)
    {
        builder.ToTable("DirectMessages", table =>
        {
            // Sequence của tin nhắn bắt đầu từ 1 trong từng cuộc chat riêng.
            table.HasCheckConstraint("CKDirectMessageSequence", "\"Sequence\" >= 1");
            // Tin chưa thu hồi phải có nội dung 1–2000 ký tự; tin đã thu hồi phải xóa nội dung và RevokedAt không được trước CreatedAt.
            table.HasCheckConstraint("CKDirectMessageContent", "(\"RevokedAt\" IS NULL AND \"Content\" IS NOT NULL AND length(btrim(\"Content\")) BETWEEN 1 AND 2000) OR (\"RevokedAt\" IS NOT NULL AND \"Content\" IS NULL AND \"RevokedAt\" >= \"CreatedAt\")");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(message => message.Content).HasMaxLength(2000);

        builder.Property(message => message.RevokedAt).HasColumnType("timestamp with time zone");

        builder.HasOne(message => message.Conversation)
            .WithMany(conversation => conversation.Messages)
            .HasForeignKey(message => message.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(message => message.SenderUser)
            .WithMany()
            .HasForeignKey(message => message.SenderUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(message => new { message.SenderUserId, message.ClientMessageId })
            .IsUnique()
            .HasDatabaseName("UXDirectMessageSenderClientId");
            
        builder.HasIndex(message => new { message.ConversationId, message.Sequence })
            .IsUnique()
            .IsDescending(false, true)
            .HasDatabaseName("UXDirectMessageConversationSequence");
    }
}
