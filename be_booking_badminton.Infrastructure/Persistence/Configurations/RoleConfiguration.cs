using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", table =>
        {
            // Chỉ cho phép các RoleKey đã định nghĩa trong RoleKey.
            table.HasCheckConstraint("CKRoleRoleKey", "\"RoleKey\" IN ('customer', 'owner', 'admin')");
            // Tên role không được rỗng.
            table.HasCheckConstraint("CKRoleNameNotBlank", "length(btrim(\"Name\")) > 0");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(role => role.RoleKey)
            .HasConversion(key => key.Name, name => RoleKey.FromName(name, false))
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(role => role.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(role => role.RoleKey).IsUnique().HasDatabaseName("UXRoleRoleKey");
    }
}
