using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class CourtSlotBlockConfiguration : IEntityTypeConfiguration<CourtSlotBlock>
{
    public void Configure(EntityTypeBuilder<CourtSlotBlock> builder)
    {
        EntityConfiguration.Configure(builder, "court_slot_blocks");

        builder.Property(entity => entity.CourtId)
            .HasColumnName("court_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.TemplateSlotId)
            .HasColumnName("template_slot_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.BlockType)
            .HasColumnName("block_type")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<CourtSlotBlockType>())
            .IsRequired();

        builder.Property(entity => entity.BlockedDate)
            .HasColumnName("blocked_date")
            .HasColumnType("date");

        builder.Property(entity => entity.Reason)
            .HasColumnName("reason")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500);

        builder.HasOne(entity => entity.Court)
            .WithMany()
            .HasForeignKey(entity => entity.CourtId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_court_slot_blocks_court_id");

        builder.HasOne(entity => entity.TemplateSlot)
            .WithMany()
            .HasForeignKey(entity => entity.TemplateSlotId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_court_slot_blocks_template_slot_id");

        builder.ToTable("court_slot_blocks", table =>
        {
            table.HasCheckConstraint("ck_court_slot_blocks_type",
                "block_type IN ('WEEKLY', 'SPECIFIC_DATE')");
            table.HasCheckConstraint("ck_court_slot_blocks_scope",
                "(block_type = 'WEEKLY' AND blocked_date IS NULL) OR (block_type = 'SPECIFIC_DATE' AND blocked_date IS NOT NULL)");
            table.HasCheckConstraint("ck_court_slot_blocks_reason",
                "reason IS NULL OR length(btrim(reason)) > 0");
        });

        builder.HasIndex(entity => new { entity.CourtId, entity.TemplateSlotId }, "ux_court_slot_blocks_weekly")
            .HasDatabaseName("ux_court_slot_blocks_weekly")
            .IsUnique()
            .HasFilter("block_type = 'WEEKLY' AND is_deleted = false");

        builder.HasIndex(entity => new { entity.CourtId, entity.TemplateSlotId, entity.BlockedDate }, "ux_court_slot_blocks_specific_date")
            .HasDatabaseName("ux_court_slot_blocks_specific_date")
            .IsUnique()
            .HasFilter("block_type = 'SPECIFIC_DATE' AND is_deleted = false");

        builder.HasIndex(entity => new { entity.CourtId, entity.BlockedDate, entity.TemplateSlotId }, "ix_court_slot_blocks_court_date")
            .HasDatabaseName("ix_court_slot_blocks_court_date")
            .HasFilter("block_type = 'SPECIFIC_DATE' AND is_deleted = false");

        builder.HasIndex(entity => new { entity.CourtId, entity.TemplateSlotId }, "ix_court_slot_blocks_court_weekly")
            .HasDatabaseName("ix_court_slot_blocks_court_weekly")
            .HasFilter("block_type = 'WEEKLY' AND is_deleted = false");

        builder.HasIndex(entity => new { entity.TemplateSlotId, entity.CourtId }, "ix_court_slot_blocks_slot_court")
            .HasDatabaseName("ix_court_slot_blocks_slot_court")
            .HasFilter("is_deleted = false");
    }
}
