using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        EntityConfiguration.Configure(builder, "reports");

        builder.Property(entity => entity.ReporterUserId)
            .HasColumnName("reporter_user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.HandledByUserId)
            .HasColumnName("handled_by_user_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.TargetType)
            .HasColumnName("target_type")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<ReportTargetType>())
            .IsRequired();

        builder.Property(entity => entity.TargetId)
            .HasColumnName("target_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.Subject)
            .HasColumnName("subject")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entity => entity.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(4000)")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<ReportStatus>())
            .IsRequired()
            .HasDefaultValue(ReportStatus.Open)
            .HasSentinel(ReportStatus.Open);

        builder.Property(entity => entity.LastMessageAt)
            .HasColumnName("last_message_at")
            .HasColumnType("timestamptz")
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(entity => entity.ClosedAt)
            .HasColumnName("closed_at")
            .HasColumnType("timestamptz");

        builder.HasOne(entity => entity.ReporterUser)
            .WithMany()
            .HasForeignKey(entity => entity.ReporterUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_reports_reporter_user_id");

        builder.HasOne(entity => entity.HandledByUser)
            .WithMany()
            .HasForeignKey(entity => entity.HandledByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_reports_handled_by_user_id");

        builder.ToTable("reports", table =>
        {
            table.HasCheckConstraint("ck_reports_status",
                "status IN ('OPEN', 'IN_PROGRESS', 'RESOLVED', 'REJECTED', 'CANCELLED')");
            table.HasCheckConstraint("ck_reports_target_type",
                "target_type IN ('GENERAL', 'USER', 'FACILITY', 'COURT', 'BOOKING', 'FEEDBACK')");
            table.HasCheckConstraint("ck_reports_target",
                "(target_type = 'GENERAL' AND target_id IS NULL) OR (target_type <> 'GENERAL' AND target_id IS NOT NULL)");
            table.HasCheckConstraint("ck_reports_subject",
                "length(btrim(subject)) > 0");
            table.HasCheckConstraint("ck_reports_description",
                "length(btrim(description)) > 0");
            table.HasCheckConstraint("ck_reports_handler",
                "(status IN ('OPEN', 'CANCELLED') AND handled_by_user_id IS NULL) OR (status IN ('IN_PROGRESS', 'RESOLVED', 'REJECTED') AND handled_by_user_id IS NOT NULL)");
            table.HasCheckConstraint("ck_reports_closed_at",
                "(status IN ('RESOLVED', 'REJECTED', 'CANCELLED') AND closed_at IS NOT NULL) OR (status IN ('OPEN', 'IN_PROGRESS') AND closed_at IS NULL)");
            table.HasCheckConstraint("ck_reports_closed_time",
                "closed_at IS NULL OR closed_at >= created_at");
            table.HasCheckConstraint("ck_reports_last_message_time",
                "last_message_at >= created_at");
        });

        builder.HasIndex(entity => new { entity.ReporterUserId, entity.Status, entity.LastMessageAt }, "ix_reports_reporter_status_activity")
            .HasDatabaseName("ix_reports_reporter_status_activity")
            .IsDescending(false, false, true)
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.HandledByUserId, entity.Status, entity.LastMessageAt }, "ix_reports_handler_status_activity")
            .HasDatabaseName("ix_reports_handler_status_activity")
            .IsDescending(false, false, true)
            .HasFilter("handled_by_user_id IS NOT NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.Status, entity.LastMessageAt }, "ix_reports_admin_inbox")
            .HasDatabaseName("ix_reports_admin_inbox")
            .IsDescending(false, true)
            .HasFilter("status IN ('OPEN', 'IN_PROGRESS') AND is_deleted = false");

        builder.HasIndex(entity => new { entity.TargetType, entity.TargetId, entity.CreatedAt }, "ix_reports_target")
            .HasDatabaseName("ix_reports_target")
            .IsDescending(false, false, true)
            .HasFilter("target_id IS NOT NULL AND is_deleted = false");
    }
}
