using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations;

public static class EntityTypeBuilderExtensions
{
    public static IndexBuilder<TEntity> HasNameIndex<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, object?>> columns, string name) where TEntity : class
        => builder.HasIndex(columns, name).HasDatabaseName(name);
    
    //case vấn đề:
    /*
     Nếu 2 unique index khác điều kiện nhưng cùng trên một cột,
     EF không tạo 2 index mà gộp thành 1 -> lần tạo sau ghi đề lên lần tạo trước đó, 
     và index lúc đầu bị biến mất mà không báo lỗi
     */
}