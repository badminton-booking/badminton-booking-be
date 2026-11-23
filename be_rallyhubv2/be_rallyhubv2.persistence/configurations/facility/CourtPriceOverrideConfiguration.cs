using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.facility;

public sealed class CourtPriceOverrideConfiguration : BaseEntityConfiguration<CourtPriceOverride>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CourtPriceOverride> builder)
    {
        builder.ToTable("court_price_overrides", table =>
        {
            table.HasCheckConstraint("ck_court_price_overrides_price", "price >= 0");
        });

        builder.Property(x => x.Reason).HasMaxLength(200);

        // Mỗi (sân, slot, ngày) chỉ có 1 giá đè
        builder.HasNamedIndex(x => new { x.CourtId, x.TemplateSlotId, x.OverrideDate }, "ux_court_price_overrides_court_slot_date")
            .UniqueWhenActive();

        // Truy vấn availability: giá đè của 1 sân trong 1 ngày
        builder.HasNamedIndex(x => new { x.CourtId, x.OverrideDate }, "ix_court_price_overrides_court_date")
            .IncludeProperties(x => new { x.TemplateSlotId, x.Price })
            .WhenActive();

        builder.HasOne(x => x.Court)
            .WithMany()
            .HasForeignKey(x => x.CourtId);

        builder.HasOne(x => x.TemplateSlot)
            .WithMany()
            .HasForeignKey(x => x.TemplateSlotId);
    }
}
