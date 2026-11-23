using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string PreferredLocale { get; set; } = "vi";

    // ===== Relationships =====
    public User? User { get; set; }
}
