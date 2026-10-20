using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public class GroupMember : BaseEntity<Guid>
{
    public Guid GroupConversationId { get; set; }
    public GroupConversation? GroupConversation { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public GroupMemberRole Role { get; set; } = null!;
    public long JoinedSequence { get; set; }

}
