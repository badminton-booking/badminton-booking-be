using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class CourtSlotBlock : BaseEntity
{
    public Guid TemplateSlotId { get; set; }
    public Guid CourtId { get; set; }
    public CourtSlotBlockType BlockType { get; set; }
    public DateOnly? BlockedDate { get; set; }
    public string? Reason { get; set; }

    // ===== Relationships =====
    public TemplateSlot? TemplateSlot { get; set; }
    public Court? Court { get; set; }
}
