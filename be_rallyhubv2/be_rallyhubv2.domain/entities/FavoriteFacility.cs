using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class FavoriteFacility : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid FacilityId { get; set; }

    // ===== Relationships =====
    public User? User { get; set; }
    public Facility? Facility { get; set; }
}
