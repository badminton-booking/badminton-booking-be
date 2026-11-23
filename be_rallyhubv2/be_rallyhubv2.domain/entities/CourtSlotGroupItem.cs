using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class CourtSlotGroupItem : BaseEntity
{
    public Guid TemplateSlotId { get; set; }
    public Guid CourtSlotGroupId { get; set; }

    // ===== Relationships =====
    public TemplateSlot? TemplateSlot { get; set; }
    public CourtSlotGroup? CourtSlotGroup { get; set; }
}
