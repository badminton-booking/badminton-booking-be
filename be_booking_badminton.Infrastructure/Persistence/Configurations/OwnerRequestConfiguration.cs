using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class OwnerRequestConfiguration : IEntityTypeConfiguration<OwnerRequest>
{
    public void Configure(EntityTypeBuilder<OwnerRequest> builder)
    {
        EntityConfiguration.Configure(builder, "owner_requests");

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<OwnerRequestStatus>())
            .IsRequired()
            .HasDefaultValue(OwnerRequestStatus.Pending)
            .HasSentinel(OwnerRequestStatus.Pending);

        builder.Property(entity => entity.BusinessName)
            .HasColumnName("business_name")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entity => entity.LegalRepresentativeName)
            .HasColumnName("legal_representative_name")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(entity => entity.TaxCode)
            .HasColumnName("tax_code")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30);

        builder.Property(entity => entity.BusinessLicenseNumber)
            .HasColumnName("business_license_number")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100);

        builder.Property(entity => entity.BusinessLicenseImageUrl)
            .HasColumnName("business_license_image_url")
            .HasColumnType("varchar(2048)")
            .HasMaxLength(2048);

        builder.Property(entity => entity.IdentityFrontImageUrl)
            .HasColumnName("identity_front_image_url")
            .HasColumnType("varchar(2048)")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(entity => entity.IdentityBackImageUrl)
            .HasColumnName("identity_back_image_url")
            .HasColumnType("varchar(2048)")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(entity => entity.RequestNote)
            .HasColumnName("request_note")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);

        builder.Property(entity => entity.ReviewedAt)
            .HasColumnName("reviewed_at")
            .HasColumnType("timestamptz");

        builder.Property(entity => entity.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_owner_requests_user_id");

        builder.ToTable("owner_requests", table =>
        {
            table.HasCheckConstraint("ck_owner_requests_status",
                "status IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED')");
            table.HasCheckConstraint("ck_owner_requests_business_name",
                "length(btrim(business_name)) > 0");
            table.HasCheckConstraint("ck_owner_requests_legal_name",
                "length(btrim(legal_representative_name)) > 0");
            table.HasCheckConstraint("ck_owner_requests_review_state",
                "(status IN ('PENDING', 'CANCELLED') AND reviewed_at IS NULL AND rejection_reason IS NULL) OR (status = 'APPROVED' AND reviewed_at IS NOT NULL AND rejection_reason IS NULL) OR (status = 'REJECTED' AND reviewed_at IS NOT NULL AND rejection_reason IS NOT NULL)");
            table.HasCheckConstraint("ck_owner_requests_business_license",
                "(business_license_number IS NULL) = (business_license_image_url IS NULL)");
        });

        builder.HasIndex(entity => entity.UserId, "ux_owner_requests_one_pending_per_user")
            .HasDatabaseName("ux_owner_requests_one_pending_per_user")
            .IsUnique()
            .HasFilter("status = 'PENDING' AND is_deleted = false");

        builder.HasIndex(entity => entity.UserId, "ux_owner_requests_one_approved_per_user")
            .HasDatabaseName("ux_owner_requests_one_approved_per_user")
            .IsUnique()
            .HasFilter("status = 'APPROVED' AND is_deleted = false");

        builder.HasIndex(entity => entity.TaxCode, "ux_owner_requests_tax_code_approved")
            .HasDatabaseName("ux_owner_requests_tax_code_approved")
            .IsUnique()
            .HasFilter("tax_code IS NOT NULL AND status = 'APPROVED' AND is_deleted = false");

        builder.HasIndex(entity => entity.BusinessLicenseNumber, "ux_owner_requests_business_license_approved")
            .HasDatabaseName("ux_owner_requests_business_license_approved")
            .IsUnique()
            .HasFilter("business_license_number IS NOT NULL AND status = 'APPROVED' AND is_deleted = false");

        builder.HasIndex(entity => new { entity.Status, entity.CreatedAt }, "ix_owner_requests_status_created_at")
            .HasDatabaseName("ix_owner_requests_status_created_at")
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.UserId, entity.CreatedAt }, "ix_owner_requests_user_created_at")
            .HasDatabaseName("ix_owner_requests_user_created_at")
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");
    }
}
