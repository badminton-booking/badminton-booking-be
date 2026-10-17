using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class OwnerProfileConfiguration : IEntityTypeConfiguration<OwnerProfile>
{
    public void Configure(EntityTypeBuilder<OwnerProfile> builder)
    {
        EntityConfiguration.Configure(builder, "owner_profiles");

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.ApprovedOwnerRequestId)
            .HasColumnName("approved_owner_request_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.BusinessName)
            .HasColumnName("business_name")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entity => entity.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(2000)")
            .HasMaxLength(2000);

        builder.Property(entity => entity.LogoUrl)
            .HasColumnName("logo_url")
            .HasColumnType("varchar(2048)")
            .HasMaxLength(2048);

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<OwnerProfileStatus>())
            .IsRequired()
            .HasDefaultValue(OwnerProfileStatus.Active)
            .HasSentinel(OwnerProfileStatus.Active);

        builder.Property(entity => entity.SuspendedAt)
            .HasColumnName("suspended_at")
            .HasColumnType("timestamptz");

        builder.Property(entity => entity.SuspensionReason)
            .HasColumnName("suspension_reason")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);

        builder.HasOne(entity => entity.User)
            .WithOne()
            .HasForeignKey<OwnerProfile>(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_owner_profiles_user_id");

        builder.HasOne(entity => entity.ApprovedOwnerRequest)
            .WithOne()
            .HasForeignKey<OwnerProfile>(entity => entity.ApprovedOwnerRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_owner_profiles_approved_owner_request_id");

        builder.ToTable("owner_profiles", table =>
        {
            table.HasCheckConstraint("ck_owner_profiles_status",
                "status IN ('ACTIVE', 'SUSPENDED', 'DISABLED')");
            table.HasCheckConstraint("ck_owner_profiles_business_name",
                "length(btrim(business_name)) > 0");
            table.HasCheckConstraint("ck_owner_profiles_suspension",
                "(status = 'SUSPENDED' AND suspended_at IS NOT NULL AND suspension_reason IS NOT NULL) OR (status IN ('ACTIVE', 'DISABLED') AND suspended_at IS NULL AND suspension_reason IS NULL)");
        });

        builder.HasIndex(entity => entity.UserId, "ux_owner_profiles_user_id")
            .HasDatabaseName("ux_owner_profiles_user_id")
            .IsUnique();

        builder.HasIndex(entity => entity.ApprovedOwnerRequestId, "ux_owner_profiles_approved_request_id")
            .HasDatabaseName("ux_owner_profiles_approved_request_id")
            .IsUnique();

        builder.HasIndex(entity => entity.Status, "ix_owner_profiles_status_active")
            .HasDatabaseName("ix_owner_profiles_status_active")
            .HasFilter("is_deleted = false");
    }
}
