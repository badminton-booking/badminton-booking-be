using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class FavoriteCourtConfiguration : IEntityTypeConfiguration<FavoriteCourt>
{
    public void Configure(EntityTypeBuilder<FavoriteCourt> builder)
    {
        EntityConfiguration.Configure(builder, "favorite_courts");

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.CourtId)
            .HasColumnName("court_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_favorite_courts_user_id");

        builder.HasOne(entity => entity.Court)
            .WithMany()
            .HasForeignKey(entity => entity.CourtId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_favorite_courts_court_id");

        builder.HasIndex(entity => new { entity.UserId, entity.CourtId }, "ux_favorite_courts_user_court")
            .HasDatabaseName("ux_favorite_courts_user_court")
            .IsUnique();

        builder.HasIndex(entity => new { entity.UserId, entity.CreatedAt, entity.CourtId }, "ix_favorite_courts_user_created_at")
            .HasDatabaseName("ix_favorite_courts_user_created_at")
            .IsDescending(false, true, false)
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.CourtId, entity.UserId }, "ix_favorite_courts_court_user")
            .HasDatabaseName("ix_favorite_courts_court_user")
            .HasFilter("is_deleted = false");
    }
}
