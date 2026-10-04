namespace be_booking_badminton.API.Contracts;

public sealed record ApiErrorResponse(
    string Title,
    int Status,
    string Detail,
    string MessageCode,
    object? Errors,
    string? TraceId,
    DateTimeOffset TimestampUtc);
