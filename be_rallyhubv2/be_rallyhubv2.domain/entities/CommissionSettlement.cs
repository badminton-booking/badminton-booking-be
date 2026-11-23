using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class CommissionSettlement : BaseEntity
{
    public Guid OwnerProfileId { get; set; }
    public string SettlementCode { get; set; } = null!;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public long TotalAmount { get; set; }
    public SettlementStatus Status { get; set; } = SettlementStatus.Pending;
    public DateOnly? DueDate { get; set; }
    public DateTimeOffset? PaidAt { get; set; }

    // ===== Relationships =====
    public OwnerProfile? OwnerProfile { get; set; }
    public ICollection<Commission> Commissions { get; set; } = new List<Commission>();
}
