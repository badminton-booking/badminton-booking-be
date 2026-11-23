using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class CourtPriceOverride : BaseEntity
{
    public Guid CourtId { get; set; }
    public Guid TemplateSlotId { get; set; }
    public DateOnly OverrideDate { get; set; }
    public long Price { get; set; }
    public string? Reason { get; set; }

    // ===== Relationships =====
    public Court? Court { get; set; }
    public TemplateSlot? TemplateSlot { get; set; }
}
