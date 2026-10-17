using be_booking_badminton.Domain.Common;
using be_booking_badminton.Domain.Enums;

namespace be_booking_badminton.Domain.Entities;

public sealed class Role : BaseEntity<Guid>
{
    public Role()
    {
        Id = Guid.NewGuid();
    }

    public RoleKey RoleKey { get; set; }
    public string Name { get; set; } = string.Empty;
}
