using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class CampaignCode : BaseEntity
{
    public Guid CampaignId { get; set; }
    public string Code { get; set; } = null!;
    public CampaignCodeVisibility Visibility { get; set; }
    public CampaignCodeStatus Status { get; set; } = CampaignCodeStatus.Active;
    public int? UsageLimit { get; set; }
    public int? PerUserLimit { get; set; }
    public int UsedCount { get; set; }

    // ===== Relationships =====
    public Campaign? Campaign { get; set; }
}
