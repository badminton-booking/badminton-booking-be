using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class Campaign : BaseEntity
{
    public Guid? OwnerProfileId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public CampaignDiscountType DiscountType { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public long? DiscountAmount { get; set; }
    public long MinOrderAmount { get; set; }
    public long? MaxOrderAmount { get; set; }
    public long? MinDiscountAmount { get; set; }
    public long? MaxDiscountAmount { get; set; }
    public CampaignEligibility Eligibility { get; set; } = CampaignEligibility.All;
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

    // ===== Relationships =====
    public OwnerProfile? OwnerProfile { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<CampaignCode> Codes { get; set; } = new List<CampaignCode>();
    public ICollection<CampaignTarget> Targets { get; set; } = new List<CampaignTarget>();
}
