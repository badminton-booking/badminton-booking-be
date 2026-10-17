using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class User : BaseEntity<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
    }

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.PendingVerification;
    public DateTimeOffset? EmailVerifiedAt { get; set; }
}
