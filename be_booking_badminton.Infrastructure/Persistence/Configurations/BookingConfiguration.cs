using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        EntityConfiguration.Configure(builder, "bookings");

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.BookingCode)
            .HasColumnName("booking_code")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(entity => entity.CampaignCodeId)
            .HasColumnName("campaign_code_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .HasConversion(new EnumStorageConverter<BookingStatus>())
            .IsRequired()
            .HasDefaultValue(BookingStatus.PendingPayment)
            .HasSentinel(BookingStatus.PendingPayment);

        builder.Property(entity => entity.SubtotalAmount)
            .HasColumnName("subtotal_amount")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(entity => entity.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasColumnType("bigint")
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(entity => entity.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(entity => entity.CampaignSnapshot)
            .HasColumnName("campaign_snapshot")
            .HasColumnType("jsonb");

        builder.Property(entity => entity.PaymentQrUrl)
            .HasColumnName("payment_qr_url")
            .HasColumnType("varchar(2048)")
            .HasMaxLength(2048);

        builder.Property(entity => entity.PaymentExpiresAt)
            .HasColumnName("payment_expires_at")
            .HasColumnType("timestamptz");

        builder.Property(entity => entity.Note)
            .HasColumnName("note")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);

        builder.Property(entity => entity.CancelledAt)
            .HasColumnName("cancelled_at")
            .HasColumnType("timestamptz");

        builder.Property(entity => entity.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_bookings_user_id");

        builder.HasOne(entity => entity.CampaignCode)
            .WithMany()
            .HasForeignKey(entity => entity.CampaignCodeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_bookings_campaign_code_id");

        builder.ToTable("bookings", table =>
        {
            table.HasCheckConstraint("ck_bookings_status",
                "status IN ('PENDING_PAYMENT', 'CONFIRMED', 'CANCELLED', 'COMPLETED', 'EXPIRED')");
            table.HasCheckConstraint("ck_bookings_code",
                "booking_code = upper(btrim(booking_code)) AND booking_code ~ '^[A-Z0-9_-]{6,30}$'");
            table.HasCheckConstraint("ck_bookings_amounts",
                "subtotal_amount >= 0 AND discount_amount >= 0 AND discount_amount <= subtotal_amount AND total_amount = subtotal_amount - discount_amount");
            table.HasCheckConstraint("ck_bookings_campaign",
                "(campaign_code_id IS NULL AND campaign_snapshot IS NULL AND discount_amount = 0) OR (campaign_code_id IS NOT NULL AND campaign_snapshot IS NOT NULL AND jsonb_typeof(campaign_snapshot) = 'object' AND discount_amount > 0)");
            table.HasCheckConstraint("ck_bookings_campaign_snapshot_keys",
                "campaign_snapshot IS NULL OR (campaign_snapshot ?& ARRAY['code', 'discount_type', 'discount_percentage', 'discount_fixed_amount'] AND jsonb_typeof(campaign_snapshot->'code') = 'string' AND jsonb_typeof(campaign_snapshot->'discount_type') = 'string')");
            table.HasCheckConstraint("ck_bookings_discount_snapshot",
                "campaign_snapshot IS NULL OR (CASE WHEN campaign_snapshot->>'discount_type' = 'PERCENTAGE' AND jsonb_typeof(campaign_snapshot->'discount_percentage') = 'number' THEN (campaign_snapshot->>'discount_percentage')::numeric > 0 AND (campaign_snapshot->>'discount_percentage')::numeric <= 100 AND campaign_snapshot->'discount_fixed_amount' = 'null'::jsonb WHEN campaign_snapshot->>'discount_type' = 'FIXED_AMOUNT' AND jsonb_typeof(campaign_snapshot->'discount_fixed_amount') = 'number' THEN (campaign_snapshot->>'discount_fixed_amount')::numeric > 0 AND campaign_snapshot->'discount_percentage' = 'null'::jsonb ELSE false END) IS TRUE");
            table.HasCheckConstraint("ck_bookings_payment_data",
                "(total_amount > 0 AND payment_qr_url IS NOT NULL AND payment_expires_at IS NOT NULL) OR (total_amount = 0 AND payment_qr_url IS NULL AND payment_expires_at IS NULL)");
            table.HasCheckConstraint("ck_bookings_payment_expiration",
                "payment_expires_at IS NULL OR payment_expires_at > created_at");
            table.HasCheckConstraint("ck_bookings_note",
                "note IS NULL OR length(btrim(note)) > 0");
            table.HasCheckConstraint("ck_bookings_cancellation",
                "(status = 'CANCELLED' AND cancelled_at IS NOT NULL AND cancellation_reason IS NOT NULL) OR (status <> 'CANCELLED' AND cancelled_at IS NULL AND cancellation_reason IS NULL)");
            table.HasCheckConstraint("ck_bookings_cancelled_time",
                "cancelled_at IS NULL OR cancelled_at >= created_at");
        });

        builder.HasIndex(entity => entity.BookingCode, "ux_bookings_booking_code")
            .HasDatabaseName("ux_bookings_booking_code")
            .IsUnique();

        builder.HasIndex(entity => new { entity.UserId, entity.Status, entity.CreatedAt }, "ix_bookings_user_status_created_at")
            .HasDatabaseName("ix_bookings_user_status_created_at")
            .IsDescending(false, false, true)
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.PaymentExpiresAt, entity.Id }, "ix_bookings_pending_expiration")
            .HasDatabaseName("ix_bookings_pending_expiration")
            .HasFilter("status = 'PENDING_PAYMENT' AND is_deleted = false");

        builder.HasIndex(entity => new { entity.Status, entity.CreatedAt }, "ix_bookings_status_created_at")
            .HasDatabaseName("ix_bookings_status_created_at")
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.CampaignCodeId, entity.CreatedAt }, "ix_bookings_campaign_code")
            .HasDatabaseName("ix_bookings_campaign_code")
            .IsDescending(false, true)
            .HasFilter("campaign_code_id IS NOT NULL AND is_deleted = false");
    }
}
