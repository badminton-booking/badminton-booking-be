using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.facility;

public sealed class FacilityConfiguration : BaseEntityConfiguration<Facility>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Facility> builder)
    {
        builder.ToTable("facilities", table =>
        {
            table.HasCheckConstraint("ck_facilities_latitude", "latitude IS NULL OR latitude BETWEEN -90 AND 90");
            table.HasCheckConstraint("ck_facilities_longitude", "longitude IS NULL OR longitude BETWEEN -180 AND 180");
            table.HasCheckConstraint(
                "ck_facilities_location_pair",
                "(latitude IS NULL AND longitude IS NULL) OR (latitude IS NOT NULL AND longitude IS NOT NULL)");
        });

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.AddressLine).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Province).IsRequired().HasMaxLength(100);
        builder.Property(x => x.District).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Latitude).HasPrecision(9, 6);
        builder.Property(x => x.Longitude).HasPrecision(9, 6);
        builder.Property(x => x.RejectionReason).HasMaxLength(1000);

        // Owner xem danh sách cơ sở của mình theo trạng thái
        builder.HasNamedIndex(x => new { x.OwnerProfileId, x.Status, x.CreatedAt }, "ix_facilities_owner_status_created_at")
            .IsDescending(false, false, true)
            .WhenActive();

        // Hộp thư Admin: cơ sở chờ duyệt
        builder.HasNamedIndex(x => new { x.Status, x.CreatedAt }, "ix_facilities_status_created_at")
            .IsDescending(false, true)
            .WhenActive();

        // Khách tìm sân theo khu vực (lọc tỉnh/thành, quận/huyện) - chỉ các cơ sở đang mở bán
        builder.HasNamedIndex(x => new { x.Province, x.District }, "ix_facilities_active_area")
            .WhenActive("status = 'ACTIVE'");

        builder.HasOne(x => x.OwnerProfile)
            .WithMany(owner => owner.Facilities)
            .HasForeignKey(x => x.OwnerProfileId);
    }
}
