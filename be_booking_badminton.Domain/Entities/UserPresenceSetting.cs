using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public class UserPresenceSetting : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public PresenceVisibility Visibility { get; set; } = PresenceVisibility.Visible;
}
