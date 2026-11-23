using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class OwnerRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public OwnerRequestStatus Status { get; set; } = OwnerRequestStatus.Pending;
    public string BusinessName { get; set; } = null!;
    public string LegalRepresentativeName { get; set; } = null!;
    public string? TaxCode { get; set; }
    public string? BusinessLicenseNumber { get; set; }
    public string? BusinessLicenseImageUrl { get; set; }
    public string IdentityFrontImageUrl { get; set; } = null!;
    public string IdentityBackImageUrl { get; set; } = null!;
    public string? RequestNote { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }

    // ===== Relationships =====
    public User? User { get; set; }
}
