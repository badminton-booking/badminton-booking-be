using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class ScheduleTemplateConfiguration : IEntityTypeConfiguration<ScheduleTemplate>
{
    public void Configure(EntityTypeBuilder<ScheduleTemplate> builder)
    {
        EntityConfiguration.Configure(builder, "schedule_templates");

        builder.Property(entity => entity.FacilityId)
            .HasColumnName("facility_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(entity => entity.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);

        builder.Property(entity => entity.Version)
            .HasColumnName("version")
            .HasColumnType("integer")
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(entity => entity.SlotDurationMinutes)
            .HasColumnName("slot_duration_minutes")
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<ScheduleTemplateStatus>())
            .IsRequired()
            .HasDefaultValue(ScheduleTemplateStatus.Draft)
            .HasSentinel(ScheduleTemplateStatus.Draft);

        builder.HasOne(entity => entity.Facility)
            .WithMany()
            .HasForeignKey(entity => entity.FacilityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_schedule_templates_facility_id");

        builder.ToTable("schedule_templates", table =>
        {
            table.HasCheckConstraint("ck_schedule_templates_status",
                "status IN ('DRAFT', 'ACTIVE', 'ARCHIVED')");
            table.HasCheckConstraint("ck_schedule_templates_name",
                "length(btrim(name)) > 0");
            table.HasCheckConstraint("ck_schedule_templates_version",
                "version > 0");
            table.HasCheckConstraint("ck_schedule_templates_duration",
                "slot_duration_minutes BETWEEN 5 AND 720");
        });

        builder.HasIndex(entity => new { entity.FacilityId, entity.Name, entity.Version }, "ux_schedule_templates_facility_name_version")
            .HasDatabaseName("ux_schedule_templates_facility_name_version")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.FacilityId, entity.Status, entity.CreatedAt }, "ix_schedule_templates_facility_status")
            .HasDatabaseName("ix_schedule_templates_facility_status")
            .IsDescending(false, false, true)
            .HasFilter("is_deleted = false");
    }
}
