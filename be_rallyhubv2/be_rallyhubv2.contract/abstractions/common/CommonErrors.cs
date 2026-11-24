using be_rallyhubv2.contract.abstractions.results;

namespace be_rallyhubv2.contract.abstractions.common;

public static class CommonErrors
{
    public static readonly Error ValidationFailed = Error.Validation("Validation.Failed");
    public static readonly Error Unauthorized = Error.Unauthorized("Auth.Unauthorized");
    public static readonly Error Forbidden = Error.Forbidden("Auth.Forbidden");
    public static readonly Error ConcurrencyConflict = Error.Conflict("Common.ConcurrencyConflict");
    public static readonly Error Unexpected = Error.Failure("Common.Unexpected");
    public static readonly Error RateLimited = Error.Failure("Common.RateLimited");
    public static readonly Error OriginNotAllowed = Error.Forbidden("Common.OriginNotAllowed");

    public static Error NotFound(string resource) => Error.NotFound("Common.NotFound", resource);
    public static Error Conflict() => Error.Conflict("Common.Conflict");
}