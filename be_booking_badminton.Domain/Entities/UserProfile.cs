using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public class UserProfile : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public required string FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }

    public User? User { get; set; }
}
