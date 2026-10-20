using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public class UserRole : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
}
