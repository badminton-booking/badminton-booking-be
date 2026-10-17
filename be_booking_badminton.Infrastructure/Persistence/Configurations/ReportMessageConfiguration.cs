using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class ReportMessageConfiguration : IEntityTypeConfiguration<ReportMessage>
{
    public void Configure(EntityTypeBuilder<ReportMessage> builder)
    {
        EntityConfiguration.Configure(builder, "report_messages");

        builder.Property(entity => entity.ReportId)
            .HasColumnName("report_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.SenderUserId)
            .HasColumnName("sender_user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Content)
            .HasColumnName("content")
            .HasColumnType("varchar(4000)")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(entity => entity.ReadAt)
            .HasColumnName("read_at")
            .HasColumnType("timestamptz");

        builder.HasOne(entity => entity.Report)
            .WithMany()
            .HasForeignKey(entity => entity.ReportId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_report_messages_report_id");

        builder.HasOne(entity => entity.SenderUser)
            .WithMany()
            .HasForeignKey(entity => entity.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_report_messages_sender_user_id");

        builder.ToTable("report_messages", table =>
        {
            table.HasCheckConstraint("ck_report_messages_content",
                "length(btrim(content)) > 0");
            table.HasCheckConstraint("ck_report_messages_read_time",
                "read_at IS NULL OR read_at >= created_at");
        });

        builder.HasIndex(entity => new { entity.ReportId, entity.CreatedAt, entity.Id }, "ix_report_messages_report_created_at")
            .HasDatabaseName("ix_report_messages_report_created_at")
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.ReportId, entity.CreatedAt }, "ix_report_messages_report_unread")
            .HasDatabaseName("ix_report_messages_report_unread")
            .HasFilter("read_at IS NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.SenderUserId, entity.CreatedAt }, "ix_report_messages_sender_created_at")
            .HasDatabaseName("ix_report_messages_sender_created_at")
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");
    }
}
