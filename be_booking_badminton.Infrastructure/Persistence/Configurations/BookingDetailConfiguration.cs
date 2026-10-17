using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class BookingDetailConfiguration : IEntityTypeConfiguration<BookingDetail>
{
    public void Configure(EntityTypeBuilder<BookingDetail> builder)
    {
        EntityConfiguration.Configure(builder, "booking_details");

        builder.Property(entity => entity.BookingId)
            .HasColumnName("booking_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.CourtId)
            .HasColumnName("court_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .HasConversion(new EnumStorageConverter<BookingDetailStatus>())
            .IsRequired()
            .HasDefaultValue(BookingDetailStatus.PendingPayment)
            .HasSentinel(BookingDetailStatus.PendingPayment);

        builder.Property(entity => entity.StartAt)
            .HasColumnName("start_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(entity => entity.EndAt)
            .HasColumnName("end_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(entity => entity.OriginalAmount)
            .HasColumnName("original_amount")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(entity => entity.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasColumnType("bigint")
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(entity => entity.FinalAmount)
            .HasColumnName("final_amount")
            .HasColumnType("bigint")
            .IsRequired();

        builder.HasOne(entity => entity.Booking)
            .WithMany()
            .HasForeignKey(entity => entity.BookingId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_booking_details_booking_id");

        builder.HasOne(entity => entity.Court)
            .WithMany()
            .HasForeignKey(entity => entity.CourtId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_booking_details_court_id");

        builder.ToTable("booking_details", table =>
        {
            table.HasCheckConstraint("ck_booking_details_status",
                "status IN ('PENDING_PAYMENT', 'CONFIRMED', 'CANCELLED', 'COMPLETED', 'EXPIRED')");
            table.HasCheckConstraint("ck_booking_details_time",
                "end_at > start_at");
            table.HasCheckConstraint("ck_booking_details_amounts",
                "original_amount >= 0 AND discount_amount >= 0 AND discount_amount <= original_amount AND final_amount = original_amount - discount_amount");
        });

        builder.HasIndex(entity => new { entity.BookingId, entity.Status, entity.StartAt, entity.Id }, "ix_booking_details_booking_status_start")
            .HasDatabaseName("ix_booking_details_booking_status_start")
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.CourtId, entity.StartAt, entity.EndAt }, "ix_booking_details_court_time_active")
            .HasDatabaseName("ix_booking_details_court_time_active")
            .HasFilter("status IN ('PENDING_PAYMENT', 'CONFIRMED') AND is_deleted = false");

        builder.HasIndex(entity => new { entity.CourtId, entity.StartAt, entity.Id }, "ix_booking_details_court_history")
            .HasDatabaseName("ix_booking_details_court_history")
            .IsDescending(false, true, false)
            .HasFilter("is_deleted = false");
    }
}
