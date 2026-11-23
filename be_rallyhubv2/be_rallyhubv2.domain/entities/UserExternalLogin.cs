using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class UserExternalLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public ExternalLoginProvider Provider { get; set; }
    public string ProviderSubject { get; set; } = null!;

    // ===== Relationships =====
    public User? User { get; set; }
}
