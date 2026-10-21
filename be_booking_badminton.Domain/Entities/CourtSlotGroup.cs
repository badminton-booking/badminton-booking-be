using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public sealed class CourtSlotGroup : BaseEntity<Guid>
{
    public Guid CourtId { get; set; }
    public string? Name { get; set; }

    public Court Court { get; set; } = null!;
}
