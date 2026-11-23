using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class ScheduleTemplate : BaseEntity
{
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Version { get; set; } = 1;
    public short SlotDurationMinutes { get; set; }
    public ScheduleTemplateStatus Status { get; set; } = ScheduleTemplateStatus.Draft;

    // ===== Relationships =====
    public Facility? Facility { get; set; }
    public ICollection<TemplateSlot> TemplateSlots { get; set; } = new List<TemplateSlot>();
}
