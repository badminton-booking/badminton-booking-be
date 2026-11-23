using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.report;

public sealed class ReportConfiguration : BaseEntityConfiguration<Report>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports", table =>
        {
            // GENERAL thì không có target_id; các loại còn lại bắt buộc có
            table.HasCheckConstraint(
                "ck_reports_target",
                "(target_type = 'GENERAL' AND target_id IS NULL) OR (target_type <> 'GENERAL' AND target_id IS NOT NULL)");
        });

        builder.Property(x => x.Subject).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(4000);

        // target_id là polymorphic => KHÔNG có FK vật lý

        // Hộp thư Admin: report đang mở, hoạt động gần nhất lên đầu
        builder.HasNamedIndex(x => new { x.Status, x.LastMessageAt }, "ix_reports_admin_inbox")
            .IsDescending(false, true)
            .WhenActive("status IN ('OPEN', 'IN_PROGRESS')");

        // Danh sách report của 1 người gửi
        builder.HasNamedIndex(x => new { x.ReporterUserId, x.Status, x.LastMessageAt }, "ix_reports_reporter_status_activity")
            .IsDescending(false, false, true)
            .WhenActive();

        // 2 khóa ngoại cùng trỏ tới User (người gửi + admin xử lý) => khai báo tường minh
        builder.HasOne(x => x.Reporter)
            .WithMany()
            .HasForeignKey(x => x.ReporterUserId);

        builder.HasOne(x => x.HandledByUser)
            .WithMany()
            .HasForeignKey(x => x.HandledByUserId);
    }
}
