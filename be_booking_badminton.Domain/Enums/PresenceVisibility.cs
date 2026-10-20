using Ardalis.SmartEnum;

namespace be_booking_badminton.Domain.Enums;

public class PresenceVisibility : SmartEnum<PresenceVisibility>
{
    public static readonly PresenceVisibility Visible = new("visible", 1);
    public static readonly PresenceVisibility Hidden = new("hidden", 2);

    public PresenceVisibility(string name, int value) : base(name, value)
    {
    }

    public static implicit operator PresenceVisibility(string name) => FromName(name);
    public static implicit operator PresenceVisibility(int value) => FromValue(value);
    public static implicit operator string(PresenceVisibility visibility) => visibility.Name;
    public static implicit operator int(PresenceVisibility visibility) => visibility.Value;
}
