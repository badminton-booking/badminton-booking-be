using be_rallyhubv2.domain.abstractions.repositories;
using be_rallyhubv2.domain.exceptions;
using be_rallyhubv2.persistence.errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace be_rallyhubv2.persistence.repositories;

public sealed class EfUnitOfWork(AppDbContext dbContext) : IUnitOfWork, IAsyncDisposable
{
    
    private IDbContextTransaction? _transaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("The transaction has already been started.");
        }
        
        _transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(inner: exception);
        }
        catch (DbUpdateException exception) when (PostgresErrorTranslator.TryTranslate(exception, out var translated))
        {
            throw translated;
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active database transaction exists.");

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
            return;

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await RollbackAsync(CancellationToken.None);
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is null)
            return;

        await _transaction.DisposeAsync();
        _transaction = null;
    }
}