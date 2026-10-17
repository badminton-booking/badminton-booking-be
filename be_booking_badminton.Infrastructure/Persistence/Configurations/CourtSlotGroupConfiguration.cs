using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class CourtSlotGroupConfiguration : IEntityTypeConfiguration<CourtSlotGroup>
{
    public void Configure(EntityTypeBuilder<CourtSlotGroup> builder)
    {
        EntityConfiguration.Configure(builder, "court_slot_groups");

        builder.Property(entity => entity.CourtId)
            .HasColumnName("court_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100);

        builder.HasOne(entity => entity.Court)
            .WithMany()
            .HasForeignKey(entity => entity.CourtId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_court_slot_groups_court_id");

        builder.ToTable("court_slot_groups", table =>
        {
            table.HasCheckConstraint("ck_court_slot_groups_name",
                "name IS NULL OR length(btrim(name)) > 0");
        });

        builder.HasIndex(entity => new { entity.CourtId, entity.CreatedAt }, "ix_court_slot_groups_court_created_at")
            .HasDatabaseName("ix_court_slot_groups_court_created_at")
            .HasFilter("is_deleted = false");
    }
}
