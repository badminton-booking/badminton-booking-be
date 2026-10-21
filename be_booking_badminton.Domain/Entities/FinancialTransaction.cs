using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class FinancialTransaction : BaseEntity<Guid>
{
    public Guid? BookingId { get; set; }
    public Guid? OwnerProfileId { get; set; }
    public Guid? DestinationBankAccountId { get; set; }
    public Guid? ParentTransactionId { get; set; }
    public FinancialTransactionType Type { get; set; }
    public long Amount { get; set; }
    public string? Provider { get; set; }
    public string? ProviderReference { get; set; }

    public Booking? Booking { get; set; }
    public OwnerProfile? OwnerProfile { get; set; }
    public UserBankAccount? DestinationBankAccount { get; set; }
    public FinancialTransaction? ParentTransaction { get; set; }
}
