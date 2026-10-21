using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class LedgerAccount : BaseEntity<Guid>
{
    public Guid? OwnerProfileId { get; set; }
    public LedgerAccountType AccountType { get; set; }
    public long Balance { get; set; }

    public OwnerProfile? OwnerProfile { get; set; }
}
