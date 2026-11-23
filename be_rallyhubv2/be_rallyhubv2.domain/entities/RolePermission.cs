using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    // ===== Relationships =====
    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}
