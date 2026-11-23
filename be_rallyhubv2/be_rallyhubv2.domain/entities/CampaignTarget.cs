using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class CampaignTarget : BaseEntity
{
    public Guid CampaignId { get; set; }
    public CampaignTargetType TargetType { get; set; }
    public Guid? TargetId { get; set; } // polymorphic, khong FK vat ly

    // ===== Relationships =====
    public Campaign? Campaign { get; set; }
}
