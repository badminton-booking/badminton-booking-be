using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class Booking : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid FacilityId { get; set; }
    public string BookingCode { get; set; } = null!;
    public Guid? CampaignCodeId { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
    public long SubtotalAmount { get; set; }
    public long DiscountAmount { get; set; }
    public long TotalAmount { get; set; }
    public string? CampaignSnapshot { get; set; } // jsonb
    public string ContactName { get; set; } = null!;
    public string ContactPhone { get; set; } = null!;
    public string? PaymentQrUrl { get; set; }
    public DateTimeOffset? PaymentExpiresAt { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }

    // ===== Relationships =====
    public User? User { get; set; }
    public Facility? Facility { get; set; }
    public CampaignCode? CampaignCode { get; set; }
    public User? CancelledByUser { get; set; }
    public ICollection<BookingDetail> Details { get; set; } = new List<BookingDetail>();
}
