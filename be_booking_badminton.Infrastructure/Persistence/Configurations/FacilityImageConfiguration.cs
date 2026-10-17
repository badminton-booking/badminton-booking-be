using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class FacilityImageConfiguration : IEntityTypeConfiguration<FacilityImage>
{
    public void Configure(EntityTypeBuilder<FacilityImage> builder)
    {
        EntityConfiguration.Configure(builder, "facility_images");

        builder.Property(entity => entity.FacilityId)
            .HasColumnName("facility_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.ImageUrl)
            .HasColumnName("image_url")
            .HasColumnType("varchar(2048)")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(entity => entity.IsCover)
            .HasColumnName("is_cover")
            .HasColumnType("boolean")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(entity => entity.DisplayOrder)
            .HasColumnName("display_order")
            .HasColumnType("smallint")
            .IsRequired()
            .HasDefaultValue((short)0);

        builder.HasOne(entity => entity.Facility)
            .WithMany()
            .HasForeignKey(entity => entity.FacilityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_facility_images_facility_id");

        builder.ToTable("facility_images", table =>
        {
            table.HasCheckConstraint("ck_facility_images_url",
                "length(btrim(image_url)) > 0");
            table.HasCheckConstraint("ck_facility_images_display_order",
                "display_order >= 0");
        });

        builder.HasIndex(entity => entity.FacilityId, "ux_facility_images_one_cover")
            .HasDatabaseName("ux_facility_images_one_cover")
            .IsUnique()
            .HasFilter("is_cover = true AND is_deleted = false");

        builder.HasIndex(entity => new { entity.FacilityId, entity.DisplayOrder }, "ux_facility_images_display_order")
            .HasDatabaseName("ux_facility_images_display_order")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.FacilityId, entity.IsCover, entity.DisplayOrder }, "ix_facility_images_facility_order")
            .HasDatabaseName("ix_facility_images_facility_order")
            .IsDescending(false, true, false)
            .HasFilter("is_deleted = false");
    }
}
