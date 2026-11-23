using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class FacilityStaff : BaseEntity
{
    public Guid FacilityId { get; set; }
    public Guid UserId { get; set; }
    public Guid? AssignedBy { get; set; }

    // ===== Relationships =====
    public Facility? Facility { get; set; }
    public User? User { get; set; }
    public User? AssignedByUser { get; set; }
}
