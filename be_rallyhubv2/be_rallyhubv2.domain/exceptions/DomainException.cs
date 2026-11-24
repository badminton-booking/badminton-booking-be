namespace be_rallyhubv2.domain.exceptions;

public class DomainException(string code, string? message = null) : Exception(message ?? code)
{
    public string Code { get; } = code;
}