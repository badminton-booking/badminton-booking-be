using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class CourtSlotGroupItemConfiguration : IEntityTypeConfiguration<CourtSlotGroupItem>
{
    public void Configure(EntityTypeBuilder<CourtSlotGroupItem> builder)
    {
        EntityConfiguration.Configure(builder, "court_slot_group_items");

        builder.Property(entity => entity.CourtSlotGroupId)
            .HasColumnName("court_slot_group_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.TemplateSlotId)
            .HasColumnName("template_slot_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasOne(entity => entity.CourtSlotGroup)
            .WithMany()
            .HasForeignKey(entity => entity.CourtSlotGroupId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_court_slot_group_items_court_slot_group_id");

        builder.HasOne(entity => entity.TemplateSlot)
            .WithMany()
            .HasForeignKey(entity => entity.TemplateSlotId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_court_slot_group_items_template_slot_id");

        builder.HasIndex(entity => new { entity.CourtSlotGroupId, entity.TemplateSlotId }, "ux_court_slot_group_items_group_slot")
            .HasDatabaseName("ux_court_slot_group_items_group_slot")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.TemplateSlotId, entity.CourtSlotGroupId }, "ix_court_slot_group_items_slot_group")
            .HasDatabaseName("ix_court_slot_group_items_slot_group")
            .HasFilter("is_deleted = false");
    }
}
