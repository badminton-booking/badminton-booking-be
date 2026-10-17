using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class ScheduleTemplate : BaseEntity<Guid>
{
    public ScheduleTemplate()
    {
        Id = Guid.NewGuid();
    }

    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; } = 1;
    public short SlotDurationMinutes { get; set; }
    public ScheduleTemplateStatus Status { get; set; } = ScheduleTemplateStatus.Draft;

    public Facility Facility { get; set; } = null!;
}
