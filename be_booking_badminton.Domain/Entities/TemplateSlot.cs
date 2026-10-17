using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public sealed class TemplateSlot : BaseEntity<Guid>
{
    public TemplateSlot()
    {
        Id = Guid.NewGuid();
    }

    public Guid ScheduleTemplateId { get; set; }
    public short DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public long Price { get; set; }

    public ScheduleTemplate ScheduleTemplate { get; set; } = null!;
}
