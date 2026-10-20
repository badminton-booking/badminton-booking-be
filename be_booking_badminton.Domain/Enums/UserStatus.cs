using Ardalis.SmartEnum;

namespace be_booking_badminton.Domain.Enums;

public class UserStatus : SmartEnum<UserStatus>
{
    public static readonly UserStatus PendingVerification = new("pending_verification", 1);
    public static readonly UserStatus Active = new("active", 2);
    public static readonly UserStatus Suspended = new("suspended", 3);
    public static readonly UserStatus Disabled = new("disabled", 4);

    public UserStatus(string name, int value) : base(name, value)
    {
    }

    public static implicit operator UserStatus(string name) => FromName(name);
    public static implicit operator UserStatus(int value) => FromValue(value);
    public static implicit operator string(UserStatus status) => status.Name;
    public static implicit operator int(UserStatus status) => status.Value;
}
