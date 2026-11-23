using be_rallyhubv2.persistence.constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations;

public static class IndexBuilderExtensions
{
    public static IndexBuilder<TEntity> WhenActive<TEntity>(this IndexBuilder<TEntity> index, string? condition = null)
        => index.HasFilter(BuilderFilter(condition));
    
    public static IndexBuilder<TEntity> UniqueWhenActive<TEntity>(this IndexBuilder<TEntity> index, string? condition = null)
    => index.IsUnique().HasFilter(BuilderFilter(condition));
        
    private static string BuilderFilter(string? condition)
    => string.IsNullOrWhiteSpace(condition)
        ? DbFilters.NotDeleted
        : $"{condition} AND {DbFilters.NotDeleted}";
}