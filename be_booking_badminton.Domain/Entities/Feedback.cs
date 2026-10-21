using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class Feedback : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid? CourtId { get; set; }
    public Guid? BookingDetailId { get; set; }
    public FeedbackType Type { get; set; }
    public short Rating { get; set; }
    public string Content { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public Court? Court { get; set; }
    public BookingDetail? BookingDetail { get; set; }
}
