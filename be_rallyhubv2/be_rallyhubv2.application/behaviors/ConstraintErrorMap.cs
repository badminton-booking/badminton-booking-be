using be_rallyhubv2.contract.abstractions.results;
using be_rallyhubv2.contract.services.booking;
using be_rallyhubv2.contract.services.identity;

namespace be_rallyhubv2.application.behaviors;

public class ConstraintErrorMap
{
    public static readonly IReadOnlyDictionary<string, Error> Errors = new Dictionary<string, Error>
    {
        ["ex_booking_details_court_time_overlap"] = BookingErrors.SlotToken,
        ["ux_bookings_user_campaign_code"] = BookingErrors.CampaignCodeAlreadyUsed,
        ["ux_users_email"] = IdentityErrors.EmailToken,
    };
    
    public static bool TryGet(string constraintName, out Error error)
        => Errors.TryGetValue(constraintName, out error!);
}