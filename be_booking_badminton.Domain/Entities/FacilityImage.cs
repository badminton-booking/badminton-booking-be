using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public sealed class FacilityImage : BaseEntity<Guid>
{
    public Guid FacilityId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsCover { get; set; }
    public short DisplayOrder { get; set; }

    public Facility Facility { get; set; } = null!;
}
