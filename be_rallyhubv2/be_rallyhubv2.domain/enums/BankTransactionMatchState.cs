namespace be_rallyhubv2.domain.enums;

public enum BankTransactionMatchState
{
    Pending,
    MatchedBooking,
    MatchedSettlement,
    Unmatched,
    Ignored
}
