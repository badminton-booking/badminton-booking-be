using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.campaign;

public sealed class CampaignCodeConfiguration : BaseEntityConfiguration<CampaignCode>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CampaignCode> builder)
    {
        builder.ToTable("campaign_codes", table =>
        {
            table.HasCheckConstraint("ck_campaign_codes_usage_limit", "usage_limit IS NULL OR usage_limit > 0");

            // Không cho dùng vượt giới hạn tổng
            table.HasCheckConstraint(
                "ck_campaign_codes_used_count",
                "used_count >= 0 AND (usage_limit IS NULL OR used_count <= usage_limit)");

            // Khách tự gõ mã => luôn lưu 1 dạng chuẩn (chữ hoa, ký tự cho phép) vì mã có unique + dùng để tra cứu
            table.HasCheckConstraint(
                "ck_campaign_codes_code",
                "code = upper(btrim(code)) AND code ~ '^[A-Z0-9_-]{3,50}$'");
        });

        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);

        // Mã giảm giá duy nhất toàn hệ thống (khách nhập mã là tra ra đúng 1 dòng)
        builder.HasNamedIndex(x => x.Code, "ux_campaign_codes_code").IsUnique();

        // Danh sách mã công khai cho khách xem
        builder.HasNamedIndex(x => new { x.Visibility, x.Status, x.CampaignId, x.CreatedAt }, "ix_campaign_codes_public_listing")
            .IsDescending(false, false, false, true)
            .WhenActive("visibility = 'PUBLIC' AND status = 'ACTIVE'");

        builder.HasOne(x => x.Campaign)
            .WithMany(campaign => campaign.Codes)
            .HasForeignKey(x => x.CampaignId);
    }
}
