using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public class RefreshToken : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }
}
