using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class Role : BaseEntity
{
    public RoleKey RoleKey { get; set; }
    public string Name { get; set; } = null!;

    // ===== Relationships =====
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
