using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.facility;

public sealed class FacilityStaffConfiguration : BaseEntityConfiguration<FacilityStaff>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FacilityStaff> builder)
    {
        builder.ToTable("facility_staffs");

        // Mỗi nhân viên chỉ được gán 1 lần cho 1 cơ sở
        builder.HasNamedIndex(x => new { x.FacilityId, x.UserId }, "ux_facility_staffs_facility_user")
            .UniqueWhenActive();

        // Tra ngược: các cơ sở mà 1 nhân viên được phép quản lý (dùng khi kiểm tra quyền)
        builder.HasNamedIndex(x => new { x.UserId, x.FacilityId }, "ix_facility_staffs_user")
            .WhenActive();

        builder.HasOne(x => x.Facility)
            .WithMany(facility => facility.Staffs)
            .HasForeignKey(x => x.FacilityId);

        // 2 khóa ngoại cùng trỏ tới User => BẮT BUỘC khai báo tường minh
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.AssignedByUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedBy);
    }
}
