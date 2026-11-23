using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class TemplateSlot : BaseEntity
{
    public Guid ScheduleTemplateId { get; set; }
    public short DayOfWeek { get; set; } // 1=MON..7=SUN
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public long Price { get; set; }

    // ===== Relationships =====
    public ScheduleTemplate? ScheduleTemplate { get; set; }
}
