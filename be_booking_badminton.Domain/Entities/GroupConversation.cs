using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public class GroupConversation : BaseEntity<Guid>
{
    public Guid OwnerUserId { get; set; }
    public User? OwnerUser { get; set; }
    public required string Name { get; set; }
    /// <summary>Số thứ tự của tin cuối trong nhóm; bằng 0 khi chưa có tin.</summary>
    public long LastMessageSequence { get; set; }
    public DateTimeOffset? LastMessageAt { get; set; }

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<GroupInvitation> Invitations { get; set; } = new List<GroupInvitation>();
    public ICollection<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
}
