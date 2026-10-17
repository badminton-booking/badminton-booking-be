using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace be_booking_badminton.Infrastructure.Persistence.Interceptors;

/// <summary>Financial events and ledger lines are append-only, including soft deletion.</summary>
public sealed class FinancialHistoryInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        Validate(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Validate(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Validate(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries()
                     .Where(entry => entry.Entity is FinancialTransaction or LedgerEntry))
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted ||
                (entry.State == EntityState.Added &&
                 entry.Property(nameof(FinancialTransaction.IsDeleted)).CurrentValue is true))
            {
                throw new InvalidOperationException(
                    "Financial transactions and ledger entries are append-only. Record a compensating transaction instead.");
            }
        }
    }
}
