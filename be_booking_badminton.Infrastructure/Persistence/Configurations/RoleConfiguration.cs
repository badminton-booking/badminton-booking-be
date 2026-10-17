using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        EntityConfiguration.Configure(builder, "roles");

        builder.Property(entity => entity.RoleKey)
            .HasColumnName("role_key")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .HasConversion(new EnumStorageConverter<RoleKey>())
            .IsRequired();

        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.ToTable("roles", table =>
        {
            table.HasCheckConstraint("ck_roles_role_key",
                "role_key IN ('CUSTOMER', 'OWNER', 'ADMIN')");
            table.HasCheckConstraint("ck_roles_name_not_blank",
                "length(btrim(name)) > 0");
        });

        builder.HasIndex(entity => entity.RoleKey, "ux_roles_role_key")
            .HasDatabaseName("ux_roles_role_key")
            .IsUnique();
    }
}
