using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.identity;

public sealed class RoleConfiguration : BaseEntityConfiguration<Role>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        // RoleKey là enum => tự lưu CUSTOMER / OWNER / STAFF / ADMIN nhờ quy ước ở AppDbContext.
        builder.HasNamedIndex(x => x.RoleKey, "ux_roles_role_key").IsUnique();
    }
}
