using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class CampaignCodeConfiguration : IEntityTypeConfiguration<CampaignCode>
{
    public void Configure(EntityTypeBuilder<CampaignCode> builder)
    {
        EntityConfiguration.Configure(builder, "campaign_codes");

        builder.Property(entity => entity.CampaignId)
            .HasColumnName("campaign_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(entity => entity.Visibility)
            .HasColumnName("visibility")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<CampaignCodeVisibility>())
            .IsRequired();

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<CampaignCodeStatus>())
            .IsRequired()
            .HasDefaultValue(CampaignCodeStatus.Active)
            .HasSentinel(CampaignCodeStatus.Active);

        builder.Property(entity => entity.UsageLimit)
            .HasColumnName("usage_limit")
            .HasColumnType("integer");

        builder.Property(entity => entity.UsedCount)
            .HasColumnName("used_count")
            .HasColumnType("integer")
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(entity => entity.Campaign)
            .WithMany()
            .HasForeignKey(entity => entity.CampaignId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_campaign_codes_campaign_id");

        builder.ToTable("campaign_codes", table =>
        {
            table.HasCheckConstraint("ck_campaign_codes_code",
                "code = upper(btrim(code)) AND code ~ '^[A-Z0-9_-]{3,50}$'");
            table.HasCheckConstraint("ck_campaign_codes_visibility",
                "visibility IN ('PUBLIC', 'PRIVATE')");
            table.HasCheckConstraint("ck_campaign_codes_status",
                "status IN ('ACTIVE', 'DISABLED')");
            table.HasCheckConstraint("ck_campaign_codes_usage_limit",
                "usage_limit IS NULL OR usage_limit > 0");
            table.HasCheckConstraint("ck_campaign_codes_used_count",
                "used_count >= 0 AND (usage_limit IS NULL OR used_count <= usage_limit)");
        });

        builder.HasIndex(entity => entity.Code, "ux_campaign_codes_code")
            .HasDatabaseName("ux_campaign_codes_code")
            .IsUnique();

        builder.HasIndex(entity => new { entity.Visibility, entity.Status, entity.CampaignId, entity.CreatedAt }, "ix_campaign_codes_public_listing")
            .HasDatabaseName("ix_campaign_codes_public_listing")
            .IsDescending(false, false, false, true)
            .HasFilter("visibility = 'PUBLIC' AND status = 'ACTIVE' AND is_deleted = false");

        builder.HasIndex(entity => new { entity.CampaignId, entity.Status, entity.CreatedAt }, "ix_campaign_codes_campaign_status")
            .HasDatabaseName("ix_campaign_codes_campaign_status")
            .IsDescending(false, false, true)
            .HasFilter("is_deleted = false");
    }
}
