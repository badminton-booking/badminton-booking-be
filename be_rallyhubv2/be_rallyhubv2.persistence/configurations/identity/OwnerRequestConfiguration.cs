using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.identity;

public sealed class OwnerRequestConfiguration : BaseEntityConfiguration<OwnerRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<OwnerRequest> builder)
    {
        builder.ToTable("owner_requests", table =>
        {
            // Số giấy phép và ảnh giấy phép: hoặc có cả hai, hoặc không có cái nào
            table.HasCheckConstraint(
                "ck_owner_requests_business_license",
                "(business_license_number IS NULL AND business_license_image_url IS NULL) "
                + "OR (business_license_number IS NOT NULL AND business_license_image_url IS NOT NULL)");
        });

        builder.Property(x => x.BusinessName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.LegalRepresentativeName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.TaxCode).HasMaxLength(30);
        builder.Property(x => x.BusinessLicenseNumber).HasMaxLength(100);
        builder.Property(x => x.BusinessLicenseImageUrl).HasMaxLength(2048);
        builder.Property(x => x.IdentityFrontImageUrl).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.IdentityBackImageUrl).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.RequestNote).HasMaxLength(1000);
        builder.Property(x => x.RejectionReason).HasMaxLength(1000);

        // Mỗi User chỉ có tối đa 1 yêu cầu đang chờ duyệt
        builder.HasNamedIndex(x => x.UserId, "ux_owner_requests_one_pending_per_user")
            .UniqueWhenActive("status = 'PENDING'");

        // Mỗi User chỉ được duyệt thành Owner 1 lần
        builder.HasNamedIndex(x => x.UserId, "ux_owner_requests_one_approved_per_user")
            .UniqueWhenActive("status = 'APPROVED'");

        // Mã số thuế / giấy phép đã được duyệt thì không cho Owner khác dùng lại
        builder.HasNamedIndex(x => x.TaxCode, "ux_owner_requests_tax_code_approved")
            .UniqueWhenActive("tax_code IS NOT NULL AND status = 'APPROVED'");
        builder.HasNamedIndex(x => x.BusinessLicenseNumber, "ux_owner_requests_business_license_approved")
            .UniqueWhenActive("business_license_number IS NOT NULL AND status = 'APPROVED'");

        // Hộp thư Admin: danh sách yêu cầu theo trạng thái
        builder.HasNamedIndex(x => new { x.Status, x.CreatedAt }, "ix_owner_requests_status_created_at").WhenActive();

        // Lịch sử yêu cầu của 1 User (cần riêng vì các unique ở trên đều lọc theo status)
        builder.HasNamedIndex(x => new { x.UserId, x.CreatedAt }, "ix_owner_requests_user_created_at")
            .IsDescending(false, true).WhenActive();

        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
    }
}
