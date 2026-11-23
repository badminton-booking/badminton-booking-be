using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class Commission : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid OwnerProfileId { get; set; }
    public Guid FacilityId { get; set; }
    public long GrossAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public long CommissionAmount { get; set; }
    public long PlatformSubsidy { get; set; }
    public long NetPayable { get; set; }
    public CommissionStatus Status { get; set; } = CommissionStatus.Pending;
    public Guid? CommissionSettlementId { get; set; }

    // ===== Relationships =====
    public Booking? Booking { get; set; }
    public OwnerProfile? OwnerProfile { get; set; }
    public Facility? Facility { get; set; }
    public CommissionSettlement? CommissionSettlement { get; set; }
}
