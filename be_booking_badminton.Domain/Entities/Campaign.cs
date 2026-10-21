using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class Campaign : BaseEntity<Guid>
{
    public Guid CreatedByUserId { get; set; }
    public Guid? OwnerProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CampaignDiscountType DiscountType { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public long? DiscountAmount { get; set; }
    public long MinOrderAmount { get; set; }
    public long? MaxOrderAmount { get; set; }
    public long? MinDiscountAmount { get; set; }
    public long? MaxDiscountAmount { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

    public User CreatedByUser { get; set; } = null!;
    public OwnerProfile? OwnerProfile { get; set; }
}
