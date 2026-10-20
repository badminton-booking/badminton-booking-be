using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class DirectConversationConfiguration : IEntityTypeConfiguration<DirectConversation>
{
    public void Configure(EntityTypeBuilder<DirectConversation> builder)
    {
        builder.ToTable("DirectConversations", table =>
        {
            // Một user không được chat với chính mình; đây là lớp chặn cuối ở DB, đồng thời giữ unique index theo cặp user (LEAST/GREATEST) có nghĩa.
            table.HasCheckConstraint("CKDirectConversationDistinctUsers", "\"StartedByUserId\" <> \"OtherUserId\"");
            // Conversation chỉ được tạo cùng tin đầu tiên nên LastMessageSequence luôn >= 1; việc đồng bộ tin dựa trên sequence này.
            table.HasCheckConstraint("CKDirectConversationSequence", "\"LastMessageSequence\" >= 1");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(conversation => conversation.LastMessageAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(conversation => conversation.LastMessageSequence).IsRequired();

        builder.HasOne(conversation => conversation.StartedByUser)
            .WithMany()
            .HasForeignKey(conversation => conversation.StartedByUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(conversation => conversation.OtherUser)
            .WithMany()
            .HasForeignKey(conversation => conversation.OtherUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(conversation => new { conversation.StartedByUserId, conversation.LastMessageAt, conversation.Id })
            .IsDescending(false, true, true)
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IXDirectConversationStarterInbox");

        builder.HasIndex(conversation => new { conversation.OtherUserId, conversation.LastMessageAt, conversation.Id })
            .IsDescending(false, true, true)
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IXDirectConversationOtherInbox");
    }
}
