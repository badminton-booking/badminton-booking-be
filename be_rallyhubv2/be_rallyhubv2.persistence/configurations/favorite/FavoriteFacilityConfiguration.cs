using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.favorite;

public sealed class FavoriteFacilityConfiguration : BaseEntityConfiguration<FavoriteFacility>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FavoriteFacility> builder)
    {
        builder.ToTable("favorite_facilities");

        // CỐ Ý dùng unique thường (không partial): bỏ yêu thích => bật is_deleted,
        // yêu thích lại => tắt is_deleted trên CHÍNH dòng đó (không tạo dòng mới).
        builder.HasNamedIndex(x => new { x.UserId, x.FacilityId }, "ux_favorite_facilities_user_facility")
            .IsUnique();

        // Danh sách yêu thích của 1 khách (mới nhất trước)
        builder.HasNamedIndex(x => new { x.UserId, x.CreatedAt, x.FacilityId }, "ix_favorite_facilities_user_created_at")
            .IsDescending(false, true, false)
            .WhenActive();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Facility)
            .WithMany()
            .HasForeignKey(x => x.FacilityId);
    }
}
