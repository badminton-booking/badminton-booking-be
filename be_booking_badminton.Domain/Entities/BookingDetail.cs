using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class BookingDetail : BaseEntity<Guid>
{
    public Guid BookingId { get; set; }
    public Guid CourtId { get; set; }
    public BookingDetailStatus Status { get; set; } = BookingDetailStatus.PendingPayment;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public long OriginalAmount { get; set; }
    public long DiscountAmount { get; set; }
    public long FinalAmount { get; set; }

    public Booking Booking { get; set; } = null!;
    public Court Court { get; set; } = null!;
}
