namespace be_rallyhubv2.domain.exceptions;

public sealed class ConcurrencyConflictException(string? message = null, Exception? inner = null)
    : Exception(message ?? "The data was changed by another request.", inner);