using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class BankTransaction : BaseEntity
{
    public string Provider { get; set; } = null!;
    public string ProviderReference { get; set; } = null!;
    public BankTransactionDirection Direction { get; set; }
    public long Amount { get; set; }
    public string AccountNumber { get; set; } = null!;
    public string? TransferContent { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public BankTransactionMatchState MatchState { get; set; } = BankTransactionMatchState.Pending;
    public Guid? BookingId { get; set; }
    public Guid? CommissionSettlementId { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string? LastError { get; set; }

    // ===== Relationships =====
    public Booking? Booking { get; set; }
    public CommissionSettlement? CommissionSettlement { get; set; }
}
