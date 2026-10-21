using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class LedgerEntry : BaseEntity<Guid>
{
    public Guid FinancialTransactionId { get; set; }
    public Guid LedgerAccountId { get; set; }
    public LedgerEntryType EntryType { get; set; }
    public long Amount { get; set; }

    public FinancialTransaction FinancialTransaction { get; set; } = null!;
    public LedgerAccount LedgerAccount { get; set; } = null!;
}
