using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class UserBankAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public string BankCode { get; set; } = null!;
    public string AccountNumberEncrypted { get; set; } = null!;
    public string AccountNumberHash { get; set; } = null!;
    public string AccountHolderName { get; set; } = null!;
    public bool IsDefault { get; set; }

    // ===== Relationships =====
    public User? User { get; set; }
}
