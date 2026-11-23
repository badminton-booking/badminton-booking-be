using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.campaign;

public sealed class CampaignConfiguration : BaseEntityConfiguration<Campaign>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("campaigns", table =>
        {
            table.HasCheckConstraint("ck_campaigns_period", "ends_at > starts_at");

            // Giảm theo % thì phải có % (0 < x <= 100) và không có số tiền; giảm cố định thì ngược lại
            table.HasCheckConstraint(
                "ck_campaigns_discount_value",
                "(discount_type = 'PERCENTAGE' AND discount_percentage > 0 AND discount_percentage <= 100 AND discount_amount IS NULL) "
                + "OR (discount_type = 'FIXED_AMOUNT' AND discount_amount > 0 AND discount_percentage IS NULL)");

            table.HasCheckConstraint(
                "ck_campaigns_order_amount",
                "min_order_amount >= 0 AND (max_order_amount IS NULL OR max_order_amount >= min_order_amount)");

            // Mức giảm tối thiểu/tối đa (tiền) phải hợp lý
            table.HasCheckConstraint(
                "ck_campaigns_discount_amount_range",
                "(min_discount_amount IS NULL OR min_discount_amount >= 0) "
                + "AND (max_discount_amount IS NULL OR max_discount_amount > 0) "
                + "AND (min_discount_amount IS NULL OR max_discount_amount IS NULL OR max_discount_amount >= min_discount_amount)");
        });

        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DiscountPercentage).HasPrecision(5, 2);

        // Campaign của Owner (OwnerProfileId có giá trị) hoặc của Admin (null)
        builder.HasOne(x => x.OwnerProfile)
            .WithMany()
            .HasForeignKey(x => x.OwnerProfileId);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId);
    }
}
