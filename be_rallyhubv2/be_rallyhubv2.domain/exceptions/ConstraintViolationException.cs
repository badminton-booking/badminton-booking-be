namespace be_rallyhubv2.domain.exceptions;

public sealed class ConstraintViolationException(ConstraintViolationKind kind,
    string constraintName, Exception? inner = null)
: Exception($"{kind} constraint '{constraintName}' was violated.", inner)
{
    public ConstraintViolationKind Kind { get; } = kind;
    public string ConstraintName { get; } =  constraintName;
}