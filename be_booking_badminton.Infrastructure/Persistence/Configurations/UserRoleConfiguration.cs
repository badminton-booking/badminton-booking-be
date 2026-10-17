using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        EntityConfiguration.Configure(builder, "user_roles");

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_user_roles_user_id");

        builder.HasOne(entity => entity.Role)
            .WithMany()
            .HasForeignKey(entity => entity.RoleId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_user_roles_role_id");

        builder.HasIndex(entity => new { entity.UserId, entity.RoleId }, "ux_user_roles_user_role_active")
            .HasDatabaseName("ux_user_roles_user_role_active")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.RoleId, entity.UserId }, "ix_user_roles_role_user_active")
            .HasDatabaseName("ix_user_roles_role_user_active")
            .HasFilter("is_deleted = false");
    }
}
