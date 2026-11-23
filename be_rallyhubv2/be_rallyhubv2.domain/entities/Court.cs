using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class Court : BaseEntity
{
    public Guid FacilityId { get; set; }
    public Guid? ScheduleTemplateId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public CourtStatus Status { get; set; } = CourtStatus.Inactive;

    // ===== Relationships =====
    public Facility? Facility { get; set; }
    public ScheduleTemplate? ScheduleTemplate { get; set; }
    public ICollection<CourtSlotGroup> SlotGroups { get; set; } = new List<CourtSlotGroup>();
}
