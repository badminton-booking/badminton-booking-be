using Ardalis.SmartEnum;

namespace be_booking_badminton.Domain.Enums;

public class FriendshipStatus : SmartEnum<FriendshipStatus>
{
    public static readonly FriendshipStatus Pending = new("pending", 1);
    public static readonly FriendshipStatus Accepted = new("accepted", 2);
    public static readonly FriendshipStatus Rejected = new("rejected", 3);
    public static readonly FriendshipStatus Cancelled = new("cancelled", 4);

    public FriendshipStatus(string name, int value) : base(name, value)
    {
    }

    public static implicit operator FriendshipStatus(string name) => FromName(name);
    public static implicit operator FriendshipStatus(int value) => FromValue(value);
    public static implicit operator string(FriendshipStatus status) => status.Name;
    public static implicit operator int(FriendshipStatus status) => status.Value;
}
