using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class FacilityImage : BaseEntity
{
    public Guid FacilityId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsCover { get; set; }
    public short DisplayOrder { get; set; }

    // ===== Relationships =====
    public Facility? Facility { get; set; }
}
