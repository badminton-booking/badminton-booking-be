using System.Diagnostics.CodeAnalysis;
using be_rallyhubv2.domain.exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace be_rallyhubv2.persistence.errors;

internal static class PostgresErrorTranslator
{
    public static bool TryTranslate(
        DbUpdateException exception, [NotNullWhen(true)] out ConstraintViolationException? translated)
    {
        
        translated = null;

        if (exception.InnerException is not PostgresException postgres || postgres.ConstraintName is null)
            return false;

        ConstraintViolationKind? kind = postgres.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation => ConstraintViolationKind.Unique,
            PostgresErrorCodes.ExclusionViolation => ConstraintViolationKind.Exclusion,
            PostgresErrorCodes.CheckViolation => ConstraintViolationKind.Check,
            PostgresErrorCodes.ForeignKeyViolation => ConstraintViolationKind.ForeignKey,
            _ => null
        };

        if (kind is null)
            return false;
        
        translated = new ConstraintViolationException(kind.Value, postgres.ConstraintName, exception);
        return true;
    }
}