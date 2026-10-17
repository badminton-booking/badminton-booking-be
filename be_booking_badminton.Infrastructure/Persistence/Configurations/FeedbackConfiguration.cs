using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        EntityConfiguration.Configure(builder, "feedbacks");

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.CourtId)
            .HasColumnName("court_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.BookingDetailId)
            .HasColumnName("booking_detail_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.Type)
            .HasColumnName("type")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<FeedbackType>())
            .IsRequired();

        builder.Property(entity => entity.Rating)
            .HasColumnName("rating")
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(entity => entity.Content)
            .HasColumnName("content")
            .HasColumnType("varchar(2000)")
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_feedbacks_user_id");

        builder.HasOne(entity => entity.Court)
            .WithMany()
            .HasForeignKey(entity => entity.CourtId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_feedbacks_court_id");

        builder.HasOne(entity => entity.BookingDetail)
            .WithMany()
            .HasForeignKey(entity => entity.BookingDetailId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_feedbacks_booking_detail_id");

        builder.ToTable("feedbacks", table =>
        {
            table.HasCheckConstraint("ck_feedbacks_type",
                "type IN ('COURT', 'PLATFORM')");
            table.HasCheckConstraint("ck_feedbacks_rating",
                "rating BETWEEN 1 AND 5");
            table.HasCheckConstraint("ck_feedbacks_content",
                "length(btrim(content)) > 0");
            table.HasCheckConstraint("ck_feedbacks_target",
                "(type = 'COURT' AND court_id IS NOT NULL AND booking_detail_id IS NOT NULL) OR (type = 'PLATFORM' AND court_id IS NULL AND booking_detail_id IS NULL)");
        });

        builder.HasIndex(entity => new { entity.CourtId, entity.CreatedAt, entity.Id }, "ix_feedbacks_court_created_at")
            .HasDatabaseName("ix_feedbacks_court_created_at")
            .IsDescending(false, true, false)
            .IncludeProperties("Rating")
            .HasFilter("type = 'COURT' AND is_deleted = false");

        builder.HasIndex(entity => new { entity.BookingDetailId, entity.CreatedAt }, "ix_feedbacks_booking_detail")
            .HasDatabaseName("ix_feedbacks_booking_detail")
            .IsDescending(false, true)
            .HasFilter("booking_detail_id IS NOT NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.UserId, entity.Type, entity.CreatedAt }, "ix_feedbacks_user_type_created_at")
            .HasDatabaseName("ix_feedbacks_user_type_created_at")
            .IsDescending(false, false, true)
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.CreatedAt, entity.Id }, "ix_feedbacks_platform_created_at")
            .HasDatabaseName("ix_feedbacks_platform_created_at")
            .IsDescending(true, false)
            .HasFilter("type = 'PLATFORM' AND is_deleted = false");
    }
}
