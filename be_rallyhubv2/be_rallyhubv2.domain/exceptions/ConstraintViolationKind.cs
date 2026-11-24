namespace be_rallyhubv2.domain.exceptions;

public enum ConstraintViolationKind
{
    Other,
    Unique,
    Exclusion,
    Check,
    ForeignKey
}