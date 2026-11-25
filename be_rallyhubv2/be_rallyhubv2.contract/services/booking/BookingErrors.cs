using be_rallyhubv2.contract.abstractions.results;

namespace be_rallyhubv2.contract.services.booking;

public static class BookingErrors
{
    public static readonly Error SlotToken = Error.Conflict("Booking.SlotTaken");
    public static readonly Error CampaignCodeAlreadyUsed = Error.Conflict("Booking.CampaignCodeAlreadyUsed");
}