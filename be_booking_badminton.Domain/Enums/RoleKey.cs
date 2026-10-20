using Ardalis.SmartEnum;

namespace be_booking_badminton.Domain.Enums;

public class RoleKey : SmartEnum<RoleKey>
{
    public static readonly RoleKey Customer = new("customer", 1);
    public static readonly RoleKey Owner = new("owner", 2);
    public static readonly RoleKey Admin = new("admin", 3);

    public RoleKey(string name, int value) : base(name, value)
    {
    }

    public static implicit operator RoleKey(string name) => FromName(name);
    public static implicit operator RoleKey(int value) => FromValue(value);
    public static implicit operator string(RoleKey role) => role.Name;
    public static implicit operator int(RoleKey role) => role.Value;
}
