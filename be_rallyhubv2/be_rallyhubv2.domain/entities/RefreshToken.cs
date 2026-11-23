using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public Guid FamilyId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    // ===== Relationships =====
    public User? User { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }
}
