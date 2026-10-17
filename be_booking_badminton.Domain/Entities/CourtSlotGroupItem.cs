using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public sealed class CourtSlotGroupItem : BaseEntity<Guid>
{
    public CourtSlotGroupItem()
    {
        Id = Guid.NewGuid();
    }

    public Guid CourtSlotGroupId { get; set; }
    public Guid TemplateSlotId { get; set; }

    public CourtSlotGroup CourtSlotGroup { get; set; } = null!;
    public TemplateSlot TemplateSlot { get; set; } = null!;
}
