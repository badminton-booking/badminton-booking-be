using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class OwnerProfile : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid ApprovedOwnerRequestId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public OwnerProfileStatus Status { get; set; } = OwnerProfileStatus.Active;
    public DateTimeOffset? SuspendedAt { get; set; }
    public string? SuspensionReason { get; set; }

    public User User { get; set; } = null!;
    public OwnerRequest ApprovedOwnerRequest { get; set; } = null!;
}
