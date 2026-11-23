using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.facility;

public sealed class FacilityImageConfiguration : BaseEntityConfiguration<FacilityImage>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FacilityImage> builder)
    {
        builder.ToTable("facility_images", table =>
        {
            table.HasCheckConstraint("ck_facility_images_display_order", "display_order >= 0");
        });

        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.StorageKey).IsRequired().HasMaxLength(500);

        // Mỗi cơ sở chỉ có 1 ảnh bìa
        builder.HasNamedIndex(x => x.FacilityId, "ux_facility_images_one_cover")
            .UniqueWhenActive("is_cover = true");

        // Tải toàn bộ ảnh của 1 cơ sở theo thứ tự hiển thị (ảnh bìa trước).
        // Cần riêng vì index "one_cover" ở trên chỉ chứa ảnh bìa.
        builder.HasNamedIndex(x => new { x.FacilityId, x.IsCover, x.DisplayOrder }, "ix_facility_images_facility_order")
            .IsDescending(false, true, false)
            .WhenActive();

        builder.HasOne(x => x.Facility)
            .WithMany(facility => facility.Images)
            .HasForeignKey(x => x.FacilityId);
    }
}
