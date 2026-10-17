using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class CampaignTarget : BaseEntity<Guid>
{
    public CampaignTarget()
    {
        Id = Guid.NewGuid();
    }

    public Guid CampaignId { get; set; }
    public CampaignTargetType TargetType { get; set; }
    public Guid? TargetId { get; set; }

    public Campaign Campaign { get; set; } = null!;
}
