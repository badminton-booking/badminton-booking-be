using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.campaign;

public sealed class CampaignTargetConfiguration : BaseEntityConfiguration<CampaignTarget>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CampaignTarget> builder)
    {
        builder.ToTable("campaign_targets", table =>
        {
            // GLOBAL thì không có target_id; các loại còn lại bắt buộc có
            table.HasCheckConstraint(
                "ck_campaign_targets_target",
                "(target_type = 'GLOBAL' AND target_id IS NULL) OR (target_type <> 'GLOBAL' AND target_id IS NOT NULL)");
        });

        // target_id là polymorphic (trỏ tới owner/facility/court/slot tùy target_type) => KHÔNG có FK vật lý

        // Mỗi campaign chỉ có 1 dòng GLOBAL
        builder.HasNamedIndex(x => new { x.CampaignId, x.TargetType }, "ux_campaign_targets_global")
            .UniqueWhenActive("target_type = 'GLOBAL'");

        // Không áp dụng 1 campaign 2 lần cho cùng 1 đối tượng
        builder.HasNamedIndex(x => new { x.CampaignId, x.TargetType, x.TargetId }, "ux_campaign_targets_specific")
            .UniqueWhenActive("target_id IS NOT NULL");

        // Tra ngược: đối tượng này (cơ sở/sân/slot) đang có campaign nào
        builder.HasNamedIndex(x => new { x.TargetType, x.TargetId, x.CampaignId }, "ix_campaign_targets_lookup")
            .WhenActive("target_id IS NOT NULL");

        // Tải toàn bộ đối tượng áp dụng của 1 campaign.
        // Cần riêng vì 2 index unique ở trên đều là partial theo điều kiện nghiệp vụ.
        builder.HasNamedIndex(x => new { x.CampaignId, x.TargetType }, "ix_campaign_targets_campaign")
            .WhenActive();

        builder.HasOne(x => x.Campaign)
            .WithMany(campaign => campaign.Targets)
            .HasForeignKey(x => x.CampaignId);
    }
}
