using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class Court : BaseEntity<Guid>
{
    public Court()
    {
        Id = Guid.NewGuid();
    }

    public Guid FacilityId { get; set; }
    public Guid? ScheduleTemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CourtStatus Status { get; set; } = CourtStatus.Inactive;

    public Facility Facility { get; set; } = null!;
    public ScheduleTemplate? ScheduleTemplate { get; set; }
}
