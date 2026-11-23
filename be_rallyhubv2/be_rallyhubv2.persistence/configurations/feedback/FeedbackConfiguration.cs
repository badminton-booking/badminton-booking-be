using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.feedback;

public sealed class FeedbackConfiguration : BaseEntityConfiguration<Feedback>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("feedbacks", table =>
        {
            table.HasCheckConstraint("ck_feedbacks_rating", "rating BETWEEN 1 AND 5");

            // Feedback về sân: bắt buộc có court_id + booking_detail_id; feedback về nền tảng: cả hai phải null
            table.HasCheckConstraint(
                "ck_feedbacks_target",
                "(type = 'COURT' AND court_id IS NOT NULL AND booking_detail_id IS NOT NULL) "
                + "OR (type = 'PLATFORM' AND court_id IS NULL AND booking_detail_id IS NULL)");
        });

        builder.Property(x => x.Content).IsRequired().HasMaxLength(2000);

        // Mỗi lượt chơi (booking_detail) chỉ được feedback 1 lần
        builder.HasNamedIndex(x => x.BookingDetailId, "ux_feedbacks_booking_detail")
            .UniqueWhenActive("type = 'COURT'");

        // Danh sách đánh giá của 1 sân (mới nhất trước)
        builder.HasNamedIndex(x => new { x.CourtId, x.CreatedAt, x.Id }, "ix_feedbacks_court_created_at")
            .IsDescending(false, true, false)
            .WhenActive("type = 'COURT'");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Court)
            .WithMany()
            .HasForeignKey(x => x.CourtId);

        builder.HasOne(x => x.BookingDetail)
            .WithMany()
            .HasForeignKey(x => x.BookingDetailId);
    }
}
