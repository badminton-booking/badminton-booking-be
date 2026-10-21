using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class OwnerRequest : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public OwnerRequestStatus Status { get; set; } = OwnerRequestStatus.Pending;
    public string BusinessName { get; set; } = string.Empty;
    public string LegalRepresentativeName { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? BusinessLicenseNumber { get; set; }
    public string? BusinessLicenseImageUrl { get; set; }
    public string IdentityFrontImageUrl { get; set; } = string.Empty;
    public string IdentityBackImageUrl { get; set; } = string.Empty;
    public string? RequestNote { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }

    public User User { get; set; } = null!;
}
