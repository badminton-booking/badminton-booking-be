using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class CampaignTargetConfiguration : IEntityTypeConfiguration<CampaignTarget>
{
    public void Configure(EntityTypeBuilder<CampaignTarget> builder)
    {
        EntityConfiguration.Configure(builder, "campaign_targets");

        builder.Property(entity => entity.CampaignId)
            .HasColumnName("campaign_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.TargetType)
            .HasColumnName("target_type")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .HasConversion(new EnumStorageConverter<CampaignTargetType>())
            .IsRequired();

        builder.Property(entity => entity.TargetId)
            .HasColumnName("target_id")
            .HasColumnType("uuid");

        builder.HasOne(entity => entity.Campaign)
            .WithMany()
            .HasForeignKey(entity => entity.CampaignId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_campaign_targets_campaign_id");

        builder.ToTable("campaign_targets", table =>
        {
            table.HasCheckConstraint("ck_campaign_targets_type",
                "target_type IN ('GLOBAL', 'OWNER', 'FACILITY', 'COURT', 'TEMPLATE_SLOT')");
            table.HasCheckConstraint("ck_campaign_targets_target",
                "(target_type = 'GLOBAL' AND target_id IS NULL) OR (target_type <> 'GLOBAL' AND target_id IS NOT NULL)");
        });

        builder.HasIndex(entity => new { entity.CampaignId, entity.TargetType }, "ux_campaign_targets_global")
            .HasDatabaseName("ux_campaign_targets_global")
            .IsUnique()
            .HasFilter("target_type = 'GLOBAL' AND is_deleted = false");

        builder.HasIndex(entity => new { entity.CampaignId, entity.TargetType, entity.TargetId }, "ux_campaign_targets_specific")
            .HasDatabaseName("ux_campaign_targets_specific")
            .IsUnique()
            .HasFilter("target_id IS NOT NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.TargetType, entity.TargetId, entity.CampaignId }, "ix_campaign_targets_lookup")
            .HasDatabaseName("ix_campaign_targets_lookup")
            .HasFilter("target_id IS NOT NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.CampaignId, entity.TargetType }, "ix_campaign_targets_campaign")
            .HasDatabaseName("ix_campaign_targets_campaign")
            .HasFilter("is_deleted = false");
    }
}
