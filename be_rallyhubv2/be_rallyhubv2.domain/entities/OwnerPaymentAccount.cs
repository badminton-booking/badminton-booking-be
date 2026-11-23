using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class OwnerPaymentAccount : BaseEntity
{
    public Guid OwnerProfileId { get; set; }
    public string Provider { get; set; } = "SEPAY";
    public string AccountNumber { get; set; } = null!;
    public string AccountHolder { get; set; } = null!;
    public string? BankCode { get; set; }
    public string AccountRef { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    // ===== Relationships =====
    public OwnerProfile? OwnerProfile { get; set; }
}
