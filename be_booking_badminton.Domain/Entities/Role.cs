using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public class Role : BaseEntity<Guid>
{
    public RoleKey RoleKey { get; set; } = null!;
    public required string Name { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
