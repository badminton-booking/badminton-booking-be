using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public class Friendship : BaseEntity<Guid>
{
    public Guid RequesterUserId { get; set; }
    public User? RequesterUser { get; set; }
    public Guid RecipientUserId { get; set; }
    public User? RecipientUser { get; set; }
    public FriendshipStatus Status { get; set; } = null!;
    /// <summary>Thời điểm lời mời được chấp nhận, từ chối hoặc hủy; null khi đang chờ.</summary>
    public DateTimeOffset? RespondedAt { get; set; }
    /// <summary>Thời điểm gửi lời mời hoặc đổi trạng thái gần nhất, dùng sắp xếp danh sách.</summary>
    public DateTimeOffset LastActivityAt { get; set; }
}
