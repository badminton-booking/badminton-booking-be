using be_rallyhubv2.domain.common;
using be_rallyhubv2.domain.enums;

namespace be_rallyhubv2.domain.entities;

public class Facility : BaseEntity
{
    public Guid OwnerProfileId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string AddressLine { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string District { get; set; } = null!;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public FacilityStatus Status { get; set; } = FacilityStatus.Draft;
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }

    // ===== Relationships =====
    public OwnerProfile? OwnerProfile { get; set; }
    public ICollection<FacilityImage> Images { get; set; } = new List<FacilityImage>();
    public ICollection<ScheduleTemplate> ScheduleTemplates { get; set; } = new List<ScheduleTemplate>();
    public ICollection<Court> Courts { get; set; } = new List<Court>();
    public ICollection<FacilityStaff> Staffs { get; set; } = new List<FacilityStaff>();
}
