using Ardalis.SmartEnum;

namespace be_booking_badminton.Domain.Enums;

public class GroupInvitationStatus : SmartEnum<GroupInvitationStatus>
{
    public static readonly GroupInvitationStatus Pending = new("pending", 1);
    public static readonly GroupInvitationStatus Accepted = new("accepted", 2);
    public static readonly GroupInvitationStatus Rejected = new("rejected", 3);
    public static readonly GroupInvitationStatus Cancelled = new("cancelled", 4);

    public GroupInvitationStatus(string name, int value) : base(name, value)
    {
    }

    public static implicit operator GroupInvitationStatus(string name) => FromName(name);
    public static implicit operator GroupInvitationStatus(int value) => FromValue(value);
    public static implicit operator string(GroupInvitationStatus status) => status.Name;
    public static implicit operator int(GroupInvitationStatus status) => status.Value;
}
