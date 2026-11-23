using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.booking;

public sealed class BookingDetailConfiguration : BaseEntityConfiguration<BookingDetail>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BookingDetail> builder)
    {
        builder.ToTable("booking_details", table =>
        {
            table.HasCheckConstraint("ck_booking_details_time", "end_at > start_at");
            table.HasCheckConstraint(
                "ck_booking_details_amounts",
                "original_amount >= 0 AND discount_amount >= 0 AND discount_amount <= original_amount AND final_amount = original_amount - discount_amount");
        });

        // Truy vấn availability: các lượt đặt còn hiệu lực của 1 sân theo khoảng thời gian
        builder.HasNamedIndex(x => new { x.CourtId, x.StartAt, x.EndAt }, "ix_booking_details_court_time_active")
            .WhenActive("status IN ('PENDING_PAYMENT', 'CONFIRMED')");

        // Job chuyển sang COMPLETED khi đã qua giờ kết thúc
        builder.HasNamedIndex(x => new { x.EndAt, x.Id }, "ix_booking_details_to_complete")
            .WhenActive("status = 'CONFIRMED'");

        builder.HasOne(x => x.Booking)
            .WithMany(booking => booking.Details)
            .HasForeignKey(x => x.BookingId);

        builder.HasOne(x => x.Court)
            .WithMany()
            .HasForeignKey(x => x.CourtId);

        // LƯU Ý: ràng buộc chống trùng lịch (exclusion constraint) KHÔNG khai báo ở đây,
        // vì EF không hỗ trợ - sẽ thêm bằng SQL thô trong migration (xem phần D).
    }
}
