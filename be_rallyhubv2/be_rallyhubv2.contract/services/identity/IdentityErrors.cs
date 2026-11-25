using be_rallyhubv2.contract.abstractions.results;

namespace be_rallyhubv2.contract.services.identity;

public static class IdentityErrors
{
    public static readonly Error EmailToken = Error.Conflict("Identity.EmailTaken");
}