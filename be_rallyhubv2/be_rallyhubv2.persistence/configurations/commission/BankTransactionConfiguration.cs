using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.commission;

public sealed class BankTransactionConfiguration : BaseEntityConfiguration<BankTransaction>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.ToTable("bank_transactions", table =>
        {
            table.HasCheckConstraint("ck_bank_transactions_amount", "amount > 0");

            // Trạng thái khớp <=> đích khớp: khớp booking thì chỉ có booking_id; khớp đối soát thì chỉ có settlement_id;
            // các trạng thái còn lại không được có đích nào (tránh "đã khớp" mà không biết khớp vào đâu)
            table.HasCheckConstraint(
                "ck_bank_transactions_match_target",
                "(match_state = 'MATCHED_BOOKING' AND booking_id IS NOT NULL AND commission_settlement_id IS NULL) "
                + "OR (match_state = 'MATCHED_SETTLEMENT' AND commission_settlement_id IS NOT NULL AND booking_id IS NULL) "
                + "OR (match_state NOT IN ('MATCHED_BOOKING', 'MATCHED_SETTLEMENT') AND booking_id IS NULL AND commission_settlement_id IS NULL)");
        });

        builder.Property(x => x.Provider).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ProviderReference).IsRequired().HasMaxLength(255);
        builder.Property(x => x.AccountNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.TransferContent).HasMaxLength(500);
        builder.Property(x => x.LastError).HasMaxLength(500);

        // Webhook gửi lại nhiều lần cũng chỉ ghi 1 dòng (chống xử lý trùng tiền)
        builder.HasNamedIndex(x => new { x.Provider, x.ProviderReference }, "ux_bank_transactions_provider_reference")
            .IsUnique();

        // Hàng đợi của worker: giao dịch chưa xử lý
        builder.HasNamedIndex(x => new { x.CreatedAt, x.Id }, "ix_bank_transactions_unprocessed")
            .WhenActive("processed_at IS NULL");

        // Admin xử lý tay các giao dịch không khớp được
        builder.HasNamedIndex(x => x.CreatedAt, "ix_bank_transactions_unmatched")
            .IsDescending()
            .WhenActive("match_state = 'UNMATCHED'");

        builder.HasOne(x => x.Booking)
            .WithMany()
            .HasForeignKey(x => x.BookingId);

        builder.HasOne(x => x.CommissionSettlement)
            .WithMany()
            .HasForeignKey(x => x.CommissionSettlementId);
    }
}
