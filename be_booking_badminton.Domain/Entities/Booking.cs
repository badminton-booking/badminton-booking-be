using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class Booking : BaseEntity<Guid>
{
    public Booking()
    {
        Id = Guid.NewGuid();
    }

    public Guid UserId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public Guid? CampaignCodeId { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
    public long SubtotalAmount { get; set; }
    public long DiscountAmount { get; set; }
    public long TotalAmount { get; set; }
    public string? CampaignSnapshot { get; set; }
    public string? PaymentQrUrl { get; set; }
    public DateTimeOffset? PaymentExpiresAt { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public User User { get; set; } = null!;
    public CampaignCode? CampaignCode { get; set; }
}
