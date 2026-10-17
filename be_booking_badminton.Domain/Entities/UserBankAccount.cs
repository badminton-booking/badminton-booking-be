using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public sealed class UserBankAccount : BaseEntity<Guid>
{
    public UserBankAccount()
    {
        Id = Guid.NewGuid();
    }

    public Guid UserId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumberEncrypted { get; set; } = string.Empty;
    public string AccountNumberHash { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public User User { get; set; } = null!;
}
