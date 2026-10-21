using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class CampaignCode : BaseEntity<Guid>
{
    public Guid CampaignId { get; set; }
    public string Code { get; set; } = string.Empty;
    public CampaignCodeVisibility Visibility { get; set; }
    public CampaignCodeStatus Status { get; set; } = CampaignCodeStatus.Active;
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }

    public Campaign Campaign { get; set; } = null!;
}
