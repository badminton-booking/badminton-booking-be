using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class BookingDetail : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid CourtId { get; set; }
    public BookingDetailStatus Status { get; set; } = BookingDetailStatus.PendingPayment;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public long OriginalAmount { get; set; }
    public long DiscountAmount { get; set; }
    public long FinalAmount { get; set; }

    // ===== Relationships =====
    public Booking? Booking { get; set; }
    public Court? Court { get; set; }
}
