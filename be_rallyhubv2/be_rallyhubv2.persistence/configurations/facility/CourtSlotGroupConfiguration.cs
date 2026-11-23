using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.facility;

public sealed class CourtSlotGroupConfiguration : BaseEntityConfiguration<CourtSlotGroup>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CourtSlotGroup> builder)
    {
        builder.ToTable("court_slot_groups");

        builder.Property(x => x.Name).HasMaxLength(100);

        builder.HasOne(x => x.Court)
            .WithMany(court => court.SlotGroups)
            .HasForeignKey(x => x.CourtId);
    }
}
