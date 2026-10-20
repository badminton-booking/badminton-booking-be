using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("GroupMembers", table =>
        {
            // Chỉ cho phép các vai trò đã định nghĩa trong GroupMemberRole.
            table.HasCheckConstraint("CKGroupMemberRole", "\"Role\" IN ('owner', 'member')");
            // JoinedSequence là sequence đầu tiên thành viên được đọc, luôn >= 1.
            table.HasCheckConstraint("CKGroupMemberJoinedSequence", "\"JoinedSequence\" >= 1");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(member => member.Role)
            .HasConversion(role => role.Name, name => GroupMemberRole.FromName(name, false))
            .HasMaxLength(20)
            .IsRequired();
        builder.HasOne(member => member.GroupConversation)
            .WithMany(group => group.Members)
            .HasForeignKey(member => member.GroupConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(member => member.User)
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(member => new { member.GroupConversationId, member.UserId })
            .IsUnique()
            .HasDatabaseName("UXGroupMemberGroupUser");
        builder.HasIndex(member => new { member.UserId, member.IsDeleted, member.GroupConversationId })
            .HasDatabaseName("IXGroupMemberUserActive");
        builder.HasIndex(member => new { member.GroupConversationId, member.IsDeleted, member.UserId })
            .HasDatabaseName("IXGroupMemberGroupActive");
    }
}
