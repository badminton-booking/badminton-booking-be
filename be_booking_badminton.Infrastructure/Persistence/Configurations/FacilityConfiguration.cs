using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        EntityConfiguration.Configure(builder, "facilities");

        builder.Property(entity => entity.OwnerProfileId)
            .HasColumnName("owner_profile_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entity => entity.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(2000)")
            .HasMaxLength(2000);

        builder.Property(entity => entity.PhoneNumber)
            .HasColumnName("phone_number")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20);

        builder.Property(entity => entity.AddressLine)
            .HasColumnName("address_line")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(entity => entity.Latitude)
            .HasColumnName("latitude")
            .HasColumnType("numeric(9,6)");

        builder.Property(entity => entity.Longitude)
            .HasColumnName("longitude")
            .HasColumnType("numeric(9,6)");

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .HasConversion(new EnumStorageConverter<FacilityStatus>())
            .IsRequired()
            .HasDefaultValue(FacilityStatus.Draft)
            .HasSentinel(FacilityStatus.Draft);

        builder.Property(entity => entity.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("timestamptz");

        builder.Property(entity => entity.ReviewedAt)
            .HasColumnName("reviewed_at")
            .HasColumnType("timestamptz");

        builder.Property(entity => entity.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);

        builder.Property(entity => entity.SuspendedAt)
            .HasColumnName("suspended_at")
            .HasColumnType("timestamptz");

        builder.Property(entity => entity.SuspensionReason)
            .HasColumnName("suspension_reason")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);

        builder.HasOne(entity => entity.OwnerProfile)
            .WithMany()
            .HasForeignKey(entity => entity.OwnerProfileId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_facilities_owner_profile_id");

        builder.ToTable("facilities", table =>
        {
            table.HasCheckConstraint("ck_facilities_status",
                "status IN ('DRAFT', 'PENDING_APPROVAL', 'ACTIVE', 'REJECTED', 'SUSPENDED', 'INACTIVE')");
            table.HasCheckConstraint("ck_facilities_name",
                "length(btrim(name)) > 0");
            table.HasCheckConstraint("ck_facilities_address",
                "length(btrim(address_line)) > 0");
            table.HasCheckConstraint("ck_facilities_phone",
                "phone_number IS NULL OR length(btrim(phone_number)) > 0");
            table.HasCheckConstraint("ck_facilities_location_pair",
                "(latitude IS NULL) = (longitude IS NULL)");
            table.HasCheckConstraint("ck_facilities_latitude",
                "latitude IS NULL OR latitude BETWEEN -90 AND 90");
            table.HasCheckConstraint("ck_facilities_longitude",
                "longitude IS NULL OR longitude BETWEEN -180 AND 180");
            table.HasCheckConstraint("ck_facilities_review_time",
                "reviewed_at IS NULL OR (submitted_at IS NOT NULL AND reviewed_at >= submitted_at)");
            table.HasCheckConstraint("ck_facilities_rejection",
                "(status = 'REJECTED' AND submitted_at IS NOT NULL AND reviewed_at IS NOT NULL AND rejection_reason IS NOT NULL) OR (status <> 'REJECTED' AND rejection_reason IS NULL)");
            table.HasCheckConstraint("ck_facilities_suspension",
                "(status = 'SUSPENDED' AND suspended_at IS NOT NULL AND suspension_reason IS NOT NULL) OR (status <> 'SUSPENDED' AND suspended_at IS NULL AND suspension_reason IS NULL)");
            table.HasCheckConstraint("ck_facilities_submission",
                "(status = 'DRAFT' AND submitted_at IS NULL AND reviewed_at IS NULL) OR (status = 'PENDING_APPROVAL' AND submitted_at IS NOT NULL AND reviewed_at IS NULL) OR (status IN ('ACTIVE', 'REJECTED', 'SUSPENDED', 'INACTIVE') AND submitted_at IS NOT NULL AND reviewed_at IS NOT NULL)");
        });

        builder.HasIndex(entity => new { entity.OwnerProfileId, entity.Status, entity.CreatedAt }, "ix_facilities_owner_status_created_at")
            .HasDatabaseName("ix_facilities_owner_status_created_at")
            .IsDescending(false, false, true)
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.Status, entity.CreatedAt }, "ix_facilities_status_created_at")
            .HasDatabaseName("ix_facilities_status_created_at")
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.Latitude, entity.Longitude }, "ix_facilities_active_location")
            .HasDatabaseName("ix_facilities_active_location")
            .HasFilter("status = 'ACTIVE' AND latitude IS NOT NULL AND is_deleted = false");
    }
}
