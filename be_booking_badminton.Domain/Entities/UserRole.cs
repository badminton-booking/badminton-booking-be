using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public sealed class UserRole : BaseEntity<Guid>
{
    public UserRole()
    {
        Id = Guid.NewGuid();
    }

    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
