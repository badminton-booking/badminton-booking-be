using be_rallyhubv2.domain.common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations;

public abstract class BaseEntityConfiguration<TEntity>: IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(entity => entity.CreatedAt)
            .HasDefaultValueSql("now");
        
        builder.Property(entity => entity.IsDeleted)
            .HasDefaultValue(false);
        
        builder.HasQueryFilter(entity => entity.IsDeleted == false);
        
        ConfigureEntity(builder);
    }
    
    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
    //combo abstract + protected: buộc lớp con phải triển khai phần riêng mình
    //  còn phần chung thì nó không thể quên hay làm sai
}