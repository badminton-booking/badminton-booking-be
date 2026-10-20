using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public class UserBlock : BaseEntity<Guid>
{
    public Guid BlockerUserId { get; set; }
    public User? BlockerUser { get; set; }
    public Guid BlockedUserId { get; set; }
    public User? BlockedUser { get; set; }
}
