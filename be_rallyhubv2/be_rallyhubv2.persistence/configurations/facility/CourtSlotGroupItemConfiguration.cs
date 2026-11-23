using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.facility;

public sealed class CourtSlotGroupItemConfiguration : BaseEntityConfiguration<CourtSlotGroupItem>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CourtSlotGroupItem> builder)
    {
        builder.ToTable("court_slot_group_items");

        // 1 slot chỉ xuất hiện 1 lần trong cùng 1 nhóm
        builder.HasNamedIndex(x => new { x.CourtSlotGroupId, x.TemplateSlotId }, "ux_court_slot_group_items_group_slot")
            .UniqueWhenActive();

        builder.HasOne(x => x.TemplateSlot)
            .WithMany()
            .HasForeignKey(x => x.TemplateSlotId);

        builder.HasOne(x => x.CourtSlotGroup)
            .WithMany(group => group.Items)
            .HasForeignKey(x => x.CourtSlotGroupId);
    }
}
