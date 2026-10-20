using Ardalis.SmartEnum;

namespace be_booking_badminton.Domain.Enums;

public class GroupMemberRole : SmartEnum<GroupMemberRole>
{
    public static readonly GroupMemberRole Owner = new("owner", 1);
    public static readonly GroupMemberRole Member = new("member", 2);

    public GroupMemberRole(string name, int value) : base(name, value)
    {
    }

    public static implicit operator GroupMemberRole(string name) => FromName(name);
    public static implicit operator GroupMemberRole(int value) => FromValue(value);
    public static implicit operator string(GroupMemberRole role) => role.Name;
    public static implicit operator int(GroupMemberRole role) => role.Value;
}
