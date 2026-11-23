using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.identity;

public sealed class RolePermissionConfiguration : BaseEntityConfiguration<RolePermission>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasNamedIndex(x => new { x.RoleId, x.PermissionId }, "ux_role_permissions_role_permission_active")
            .UniqueWhenActive();

        builder.HasOne(x => x.Role).WithMany(role => role.RolePermissions).HasForeignKey(x => x.RoleId);

        builder.HasOne(x => x.Permission).WithMany(permission => permission.RolePermissions).HasForeignKey(x => x.PermissionId);
    }
}
