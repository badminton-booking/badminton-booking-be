using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public sealed class UserProfile : BaseEntity<Guid>
{
    public UserProfile()
    {
        Id = Guid.NewGuid();
    }

    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }

    public User User { get; set; } = null!;
}
