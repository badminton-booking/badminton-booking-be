using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public class User : BaseEntity<Guid>
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserStatus Status { get; set; } = null!;
    public DateTimeOffset? EmailVerifiedAt { get; set; }

    public UserProfile? Profile { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
