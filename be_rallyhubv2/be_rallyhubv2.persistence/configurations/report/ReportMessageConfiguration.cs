using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.report;

public sealed class ReportMessageConfiguration : BaseEntityConfiguration<ReportMessage>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ReportMessage> builder)
    {
        builder.ToTable("report_messages");

        builder.Property(x => x.Content).IsRequired().HasMaxLength(4000);

        // Tải hội thoại của 1 report theo thứ tự thời gian
        builder.HasNamedIndex(x => new { x.ReportId, x.CreatedAt, x.Id }, "ix_report_messages_report_created_at")
            .WhenActive();

        builder.HasOne(x => x.Report)
            .WithMany(report => report.Messages)
            .HasForeignKey(x => x.ReportId);

        builder.HasOne(x => x.Sender)
            .WithMany()
            .HasForeignKey(x => x.SenderUserId);
    }
}
