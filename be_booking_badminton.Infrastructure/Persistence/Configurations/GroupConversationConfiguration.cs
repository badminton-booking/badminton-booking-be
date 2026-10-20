using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class GroupConversationConfiguration : IEntityTypeConfiguration<GroupConversation>
{
    public void Configure(EntityTypeBuilder<GroupConversation> builder)
    {
        builder.ToTable("GroupConversations", table =>
        {
            // Nhóm mới tạo chưa có tin nên LastMessageSequence bắt đầu từ 0.
            table.HasCheckConstraint("CKGroupConversationSequence", "\"LastMessageSequence\" >= 0");
            // Tên nhóm không được rỗng và tối đa 100 ký tự.
            table.HasCheckConstraint("CKGroupConversationName", "length(btrim(\"Name\")) BETWEEN 1 AND 100");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(group => group.Name).HasMaxLength(100).IsRequired();
        // Nhóm được tạo trước khi có tin nên mặc định 0; tin đầu tiên được cấp Sequence = LastMessageSequence + 1 = 1.
        builder.Property(group => group.LastMessageSequence).HasDefaultValue(0L);
        builder.Property(group => group.LastMessageAt).HasColumnType("timestamp with time zone");
        builder.HasOne(group => group.OwnerUser)
            .WithMany()
            .HasForeignKey(group => group.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(group => group.OwnerUserId).HasDatabaseName("IXGroupConversationOwnerUserId");
    }
}
