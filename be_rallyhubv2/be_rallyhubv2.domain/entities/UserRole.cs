using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    // ===== Relationships =====
    public User? User { get; set; }
    public Role? Role { get; set; }
}
