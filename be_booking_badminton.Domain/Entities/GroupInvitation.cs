using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public class GroupInvitation : BaseEntity<Guid>
{
    public Guid GroupConversationId { get; set; }
    public GroupConversation? GroupConversation { get; set; }
    public Guid InviterUserId { get; set; }
    public User? InviterUser { get; set; }
    public Guid InviteeUserId { get; set; }
    public User? InviteeUser { get; set; }
    public GroupInvitationStatus Status { get; set; } = null!;
    /// <summary>Thời điểm lời mời nhóm được chấp nhận, từ chối hoặc hủy; null khi đang chờ.</summary>
    public DateTimeOffset? RespondedAt { get; set; }

}
