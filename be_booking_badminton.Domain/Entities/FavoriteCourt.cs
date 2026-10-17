using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public sealed class FavoriteCourt : BaseEntity<Guid>
{
    public FavoriteCourt()
    {
        Id = Guid.NewGuid();
    }

    public Guid UserId { get; set; }
    public Guid CourtId { get; set; }

    public User User { get; set; } = null!;
    public Court Court { get; set; } = null!;
}
