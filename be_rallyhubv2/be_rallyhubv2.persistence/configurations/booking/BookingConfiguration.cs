using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.booking;

public sealed class BookingConfiguration : BaseEntityConfiguration<Booking>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings", table =>
        {
            // Bảo vệ tiền: tổng = tạm tính - giảm giá, giảm giá không vượt tạm tính
            table.HasCheckConstraint(
                "ck_bookings_amounts",
                "subtotal_amount >= 0 AND discount_amount >= 0 AND discount_amount <= subtotal_amount AND total_amount = subtotal_amount - discount_amount");

            // Mã booking nằm trong nội dung chuyển khoản => phải đúng định dạng thì webhook mới khớp được
            table.HasCheckConstraint(
                "ck_bookings_code",
                "booking_code = upper(btrim(booking_code)) AND booking_code ~ '^[A-Z0-9]{6,30}$'");

            // Có mã giảm giá <=> có snapshot <=> có tiền giảm; không dùng mã thì cả 3 đều "trống"
            table.HasCheckConstraint(
                "ck_bookings_campaign",
                "(campaign_code_id IS NULL AND campaign_snapshot IS NULL AND discount_amount = 0) "
                + "OR (campaign_code_id IS NOT NULL AND campaign_snapshot IS NOT NULL "
                + "AND jsonb_typeof(campaign_snapshot) = 'object' AND discount_amount > 0)");

            // Ai chịu chi phí khuyến mãi quyết định platform_subsidy của hoa hồng => chỉ nhận 2 giá trị
            table.HasCheckConstraint(
                "ck_bookings_campaign_funded_by",
                "campaign_snapshot IS NULL OR campaign_snapshot->>'funded_by' IN ('PLATFORM', 'OWNER')");
        });

        builder.Property(x => x.BookingCode).IsRequired().HasMaxLength(30);
        builder.Property(x => x.CampaignSnapshot).HasColumnType("jsonb");
        builder.Property(x => x.ContactName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.ContactPhone).IsRequired().HasMaxLength(20);
        builder.Property(x => x.PaymentQrUrl).HasMaxLength(2048);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.CancellationReason).HasMaxLength(1000);

        // Mã booking nằm trong nội dung chuyển khoản => không bao giờ được trùng (kể cả đã xóa mềm)
        builder.HasNamedIndex(x => x.BookingCode, "ux_bookings_booking_code").IsUnique();

        // Mỗi khách chỉ được dùng mỗi mã giảm giá 1 lần: booking đang giữ chỗ / đã xác nhận / đã chơi xong
        // thì tính là "đã dùng"; booking đã hủy hoặc hết hạn thì không tính (khách được dùng lại mã).
        builder.HasNamedIndex(x => new { x.UserId, x.CampaignCodeId }, "ux_bookings_user_campaign_code")
            .UniqueWhenActive("campaign_code_id IS NOT NULL AND status IN ('PENDING_PAYMENT', 'CONFIRMED', 'COMPLETED')");

        // Lịch sử đặt sân của khách
        builder.HasNamedIndex(x => new { x.UserId, x.Status, x.CreatedAt }, "ix_bookings_user_status_created_at")
            .IsDescending(false, false, true)
            .WhenActive();

        // Danh sách booking của 1 cơ sở (Owner/Staff)
        builder.HasNamedIndex(x => new { x.FacilityId, x.Status, x.CreatedAt }, "ix_bookings_facility_status_created_at")
            .IsDescending(false, false, true)
            .WhenActive();

        // Job hết hạn thanh toán quét các booking đang chờ
        builder.HasNamedIndex(x => new { x.PaymentExpiresAt, x.Id }, "ix_bookings_pending_expiration")
            .WhenActive("status = 'PENDING_PAYMENT'");

        // Đếm lượt dùng mã giảm giá (đếm từ bookings)
        builder.HasNamedIndex(x => new { x.CampaignCodeId, x.CreatedAt }, "ix_bookings_campaign_code")
            .IsDescending(false, true)
            .WhenActive("campaign_code_id IS NOT NULL");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Facility)
            .WithMany()
            .HasForeignKey(x => x.FacilityId);

        builder.HasOne(x => x.CampaignCode)
            .WithMany()
            .HasForeignKey(x => x.CampaignCodeId);

        // 2 khóa ngoại cùng trỏ tới User (người đặt + người hủy) => khai báo tường minh
        builder.HasOne(x => x.CancelledByUser)
            .WithMany()
            .HasForeignKey(x => x.CancelledByUserId);
    }
}
