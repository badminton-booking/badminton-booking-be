using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        EntityConfiguration.Configure(builder, "campaigns");

        builder.Property(entity => entity.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.OwnerProfileId)
            .HasColumnName("owner_profile_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(entity => entity.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);

        builder.Property(entity => entity.DiscountType)
            .HasColumnName("discount_type")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<CampaignDiscountType>())
            .IsRequired();

        builder.Property(entity => entity.DiscountPercentage)
            .HasColumnName("discount_percentage")
            .HasColumnType("numeric(5,2)");

        builder.Property(entity => entity.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasColumnType("bigint");

        builder.Property(entity => entity.MinOrderAmount)
            .HasColumnName("min_order_amount")
            .HasColumnType("bigint")
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(entity => entity.MaxOrderAmount)
            .HasColumnName("max_order_amount")
            .HasColumnType("bigint");

        builder.Property(entity => entity.MinDiscountAmount)
            .HasColumnName("min_discount_amount")
            .HasColumnType("bigint");

        builder.Property(entity => entity.MaxDiscountAmount)
            .HasColumnName("max_discount_amount")
            .HasColumnType("bigint");

        builder.Property(entity => entity.StartsAt)
            .HasColumnName("starts_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(entity => entity.EndsAt)
            .HasColumnName("ends_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<CampaignStatus>())
            .IsRequired()
            .HasDefaultValue(CampaignStatus.Draft)
            .HasSentinel(CampaignStatus.Draft);

        builder.HasOne(entity => entity.CreatedByUser)
            .WithMany()
            .HasForeignKey(entity => entity.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_campaigns_created_by_user_id");

        builder.HasOne(entity => entity.OwnerProfile)
            .WithMany()
            .HasForeignKey(entity => entity.OwnerProfileId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_campaigns_owner_profile_id");

        builder.ToTable("campaigns", table =>
        {
            table.HasCheckConstraint("ck_campaigns_status",
                "status IN ('DRAFT', 'ACTIVE', 'PAUSED', 'ENDED')");
            table.HasCheckConstraint("ck_campaigns_discount_type",
                "discount_type IN ('PERCENTAGE', 'FIXED_AMOUNT')");
            table.HasCheckConstraint("ck_campaigns_name",
                "length(btrim(name)) > 0");
            table.HasCheckConstraint("ck_campaigns_discount_value",
                "(discount_type = 'PERCENTAGE' AND discount_percentage IS NOT NULL AND discount_percentage > 0 AND discount_percentage <= 100 AND discount_amount IS NULL) OR (discount_type = 'FIXED_AMOUNT' AND discount_amount IS NOT NULL AND discount_amount > 0 AND discount_percentage IS NULL)");
            table.HasCheckConstraint("ck_campaigns_order_amount",
                "min_order_amount >= 0 AND (max_order_amount IS NULL OR max_order_amount >= min_order_amount)");
            table.HasCheckConstraint("ck_campaigns_discount_amount_range",
                "(min_discount_amount IS NULL OR min_discount_amount >= 0) AND (max_discount_amount IS NULL OR max_discount_amount > 0) AND (min_discount_amount IS NULL OR max_discount_amount IS NULL OR max_discount_amount >= min_discount_amount)");
            table.HasCheckConstraint("ck_campaigns_period",
                "ends_at > starts_at");
        });

        builder.HasIndex(entity => new { entity.OwnerProfileId, entity.Status, entity.StartsAt, entity.EndsAt }, "ix_campaigns_owner_status_period")
            .HasDatabaseName("ix_campaigns_owner_status_period")
            .HasFilter("owner_profile_id IS NOT NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.Status, entity.StartsAt, entity.EndsAt }, "ix_campaigns_admin_status_period")
            .HasDatabaseName("ix_campaigns_admin_status_period")
            .HasFilter("owner_profile_id IS NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.CreatedByUserId, entity.CreatedAt }, "ix_campaigns_creator_created_at")
            .HasDatabaseName("ix_campaigns_creator_created_at")
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");
    }
}
