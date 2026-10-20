using be_booking_badminton.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

internal static class EntityConfiguration
{
    public static void ConfigureBase<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity<Guid>
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(entity => entity.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.Property(entity => entity.UpdatedAt)
            .HasColumnType("timestamp with time zone");
            
        builder.Property(entity => entity.IsDeleted).HasDefaultValue(false);
    }
}
