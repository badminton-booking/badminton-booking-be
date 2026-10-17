using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class CourtSlotBlock : BaseEntity<Guid>
{
    public CourtSlotBlock()
    {
        Id = Guid.NewGuid();
    }

    public Guid CourtId { get; set; }
    public Guid TemplateSlotId { get; set; }
    public CourtSlotBlockType BlockType { get; set; }
    public DateOnly? BlockedDate { get; set; }
    public string? Reason { get; set; }

    public Court Court { get; set; } = null!;
    public TemplateSlot TemplateSlot { get; set; } = null!;
}
