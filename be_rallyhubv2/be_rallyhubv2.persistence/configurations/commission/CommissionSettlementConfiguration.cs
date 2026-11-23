using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.commission;

public sealed class CommissionSettlementConfiguration : BaseEntityConfiguration<CommissionSettlement>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CommissionSettlement> builder)
    {
        builder.ToTable("commission_settlements", table =>
        {
            table.HasCheckConstraint("ck_commission_settlements_period", "period_end >= period_start");
            table.HasCheckConstraint("ck_commission_settlements_total", "total_amount >= 0");

            // Mã đối soát nằm trong nội dung chuyển khoản => phải đúng định dạng thì webhook mới khớp được
            table.HasCheckConstraint(
                "ck_commission_settlements_code",
                "settlement_code = upper(btrim(settlement_code)) AND settlement_code ~ '^[A-Z0-9]{6,30}$'");
        });

        builder.Property(x => x.SettlementCode).IsRequired().HasMaxLength(30);

        // Mã đối soát nằm trong nội dung chuyển khoản của Owner => duy nhất tuyệt đối
        builder.HasNamedIndex(x => x.SettlementCode, "ux_commission_settlements_code").IsUnique();

        // Lịch sử đối soát của 1 Owner
        builder.HasNamedIndex(x => new { x.OwnerProfileId, x.Status, x.PeriodEnd }, "ix_commission_settlements_owner_status")
            .IsDescending(false, false, true)
            .WhenActive();

        // Job đánh dấu quá hạn (OVERDUE)
        builder.HasNamedIndex(x => x.DueDate, "ix_commission_settlements_overdue")
            .WhenActive("status = 'PENDING'");

        builder.HasOne(x => x.OwnerProfile)
            .WithMany()
            .HasForeignKey(x => x.OwnerProfileId);
    }
}
