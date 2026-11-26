using be_rallyhubv2.domain.abstractions.repositories;
using be_rallyhubv2.domain.common;
using Microsoft.EntityFrameworkCore;

namespace be_rallyhubv2.persistence.repositories;

public abstract class RepositoryBase<T>(AppDbContext dbContext): IRepositoryBase<T>
    where T : BaseEntity
{
    protected AppDbContext DbContext { get; } = dbContext;
    
    protected DbSet<T> Set => DbContext.Set<T>();
    
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => Set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(T entity) => Set.Add(entity);

    public void Remove(T entity) => Set.Remove(entity);
}