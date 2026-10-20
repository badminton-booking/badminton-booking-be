using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class GroupInvitationConfiguration : IEntityTypeConfiguration<GroupInvitation>
{
    public void Configure(EntityTypeBuilder<GroupInvitation> builder)
    {
        builder.ToTable("GroupInvitations", table =>
        {
            // Chỉ cho phép các trạng thái đã định nghĩa trong GroupInvitationStatus.
            table.HasCheckConstraint("CKGroupInvitationStatus", "\"Status\" IN ('pending', 'accepted', 'rejected', 'cancelled')");
            // Lời mời đang chờ thì chưa có RespondedAt; đã phản hồi thì bắt buộc có RespondedAt.
            table.HasCheckConstraint("CKGroupInvitationResponse", "(\"Status\" = 'pending' AND \"RespondedAt\" IS NULL) OR (\"Status\" <> 'pending' AND \"RespondedAt\" IS NOT NULL)");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(invitation => invitation.Status)
            .HasConversion(status => status.Name, name => GroupInvitationStatus.FromName(name, false))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(invitation => invitation.RespondedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(invitation => invitation.GroupConversation)
            .WithMany(group => group.Invitations)
            .HasForeignKey(invitation => invitation.GroupConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(invitation => invitation.InviterUser)
            .WithMany()
            .HasForeignKey(invitation => invitation.InviterUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(invitation => invitation.InviteeUser)
            .WithMany()
            .HasForeignKey(invitation => invitation.InviteeUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(invitation => new { invitation.GroupConversationId, invitation.InviteeUserId })
            .IsUnique()
            .HasDatabaseName("UXGroupInvitationGroupInvitee");
        builder.HasIndex(invitation => new { invitation.InviteeUserId, invitation.Status, invitation.CreatedAt, invitation.Id })
            .IsDescending(false, false, true, true)
            .HasDatabaseName("IXGroupInvitationInviteeInbox");
    }
}
