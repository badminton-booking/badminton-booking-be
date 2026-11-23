using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.identity;

public sealed class UserRoleConfiguration : BaseEntityConfiguration<UserRole>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasNamedIndex(x => new { x.UserId, x.RoleId }, "ux_user_roles_user_role_active")
            .UniqueWhenActive();

        builder.HasOne(x => x.User).WithMany(user => user.UserRoles).HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Role).WithMany(role => role.UserRoles).HasForeignKey(x => x.RoleId);
    }
}
