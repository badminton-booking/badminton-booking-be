using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.commission;

public sealed class CommissionConfiguration : BaseEntityConfiguration<Commission>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Commission> builder)
    {
        builder.ToTable("commissions", table =>
        {
            table.HasCheckConstraint("ck_commissions_rate", "commission_rate >= 0 AND commission_rate <= 100");

            // Bảo vệ tiền: phải trả = hoa hồng - khuyến mãi do sàn chịu
            table.HasCheckConstraint(
                "ck_commissions_amounts",
                "gross_amount >= 0 AND commission_amount >= 0 AND platform_subsidy >= 0 AND net_payable = commission_amount - platform_subsidy");
        });

        builder.Property(x => x.CommissionRate).HasPrecision(5, 2);

        // Mỗi Booking chỉ có đúng 1 khoản hoa hồng
        builder.HasNamedIndex(x => x.BookingId, "ux_commissions_booking")
            .UniqueWhenActive();

        // Job gom hoa hồng chờ đối soát theo Owner
        builder.HasNamedIndex(x => new { x.OwnerProfileId, x.Status, x.CreatedAt }, "ix_commissions_owner_status")
            .WhenActive();

        builder.HasOne(x => x.Booking)
            .WithOne()
            .HasForeignKey<Commission>(x => x.BookingId);

        builder.HasOne(x => x.OwnerProfile)
            .WithMany()
            .HasForeignKey(x => x.OwnerProfileId);

        builder.HasOne(x => x.Facility)
            .WithMany()
            .HasForeignKey(x => x.FacilityId);

        builder.HasOne(x => x.CommissionSettlement)
            .WithMany(settlement => settlement.Commissions)
            .HasForeignKey(x => x.CommissionSettlementId);
    }
}
