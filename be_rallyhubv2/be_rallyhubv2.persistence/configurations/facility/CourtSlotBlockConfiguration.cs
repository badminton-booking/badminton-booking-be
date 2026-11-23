using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.facility;

public sealed class CourtSlotBlockConfiguration : BaseEntityConfiguration<CourtSlotBlock>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CourtSlotBlock> builder)
    {
        builder.ToTable("court_slot_blocks", table =>
        {
            // WEEKLY thì không có ngày cụ thể; SPECIFIC_DATE thì bắt buộc có ngày
            table.HasCheckConstraint(
                "ck_court_slot_blocks_scope",
                "(block_type = 'WEEKLY' AND blocked_date IS NULL) OR (block_type = 'SPECIFIC_DATE' AND blocked_date IS NOT NULL)");
        });

        builder.Property(x => x.Reason).HasMaxLength(500);

        // Chặn hằng tuần: mỗi (sân, slot) chỉ chặn 1 lần
        builder.HasNamedIndex(x => new { x.CourtId, x.TemplateSlotId }, "ux_court_slot_blocks_weekly")
            .UniqueWhenActive("block_type = 'WEEKLY'");

        // Chặn theo ngày: mỗi (sân, slot, ngày) chỉ chặn 1 lần
        builder.HasNamedIndex(x => new { x.CourtId, x.TemplateSlotId, x.BlockedDate }, "ux_court_slot_blocks_specific_date")
            .UniqueWhenActive("block_type = 'SPECIFIC_DATE'");

        // Truy vấn availability: các slot bị chặn của 1 sân trong 1 ngày
        builder.HasNamedIndex(x => new { x.CourtId, x.BlockedDate, x.TemplateSlotId }, "ix_court_slot_blocks_court_date")
            .WhenActive("block_type = 'SPECIFIC_DATE'");

        builder.HasOne(x => x.TemplateSlot)
            .WithMany()
            .HasForeignKey(x => x.TemplateSlotId);

        builder.HasOne(x => x.Court)
            .WithMany()
            .HasForeignKey(x => x.CourtId);
    }
}
