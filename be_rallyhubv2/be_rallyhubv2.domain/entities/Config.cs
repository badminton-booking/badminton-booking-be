using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class Config : BaseEntity
{
    public ConfigScope Scope { get; set; }
    public Guid? FacilityId { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!; // jsonb

    // ===== Relationships =====
    public Facility? Facility { get; set; }
}
