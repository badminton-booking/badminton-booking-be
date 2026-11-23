using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class CourtSlotGroup : BaseEntity
{
    public Guid CourtId { get; set; }
    public string? Name { get; set; }

    // ===== Relationships =====
    public Court? Court { get; set; }
    public ICollection<CourtSlotGroupItem> Items { get; set; } = new List<CourtSlotGroupItem>();
}
