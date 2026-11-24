using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.abstractions.repositories;

public interface IRepositoryBase<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(T entity);
    void Remove(T entity);
}