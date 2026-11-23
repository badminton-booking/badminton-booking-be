using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class Permission : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;

    // ===== Relationships =====
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
