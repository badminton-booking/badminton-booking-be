using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        EntityConfiguration.Configure(builder, "courts");

        builder.Property(entity => entity.FacilityId)
            .HasColumnName("facility_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.ScheduleTemplateId)
            .HasColumnName("schedule_template_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(entity => entity.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000);

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<CourtStatus>())
            .IsRequired()
            .HasDefaultValue(CourtStatus.Inactive)
            .HasSentinel(CourtStatus.Inactive);

        builder.HasOne(entity => entity.Facility)
            .WithMany()
            .HasForeignKey(entity => entity.FacilityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_courts_facility_id");

        builder.HasOne(entity => entity.ScheduleTemplate)
            .WithMany()
            .HasForeignKey(entity => entity.ScheduleTemplateId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_courts_schedule_template_id");

        builder.ToTable("courts", table =>
        {
            table.HasCheckConstraint("ck_courts_status",
                "status IN ('ACTIVE', 'INACTIVE', 'MAINTENANCE')");
            table.HasCheckConstraint("ck_courts_name",
                "length(btrim(name)) > 0");
            table.HasCheckConstraint("ck_courts_active_template",
                "status <> 'ACTIVE' OR schedule_template_id IS NOT NULL");
        });

        builder.HasIndex(entity => new { entity.FacilityId, entity.Name }, "ux_courts_facility_name")
            .HasDatabaseName("ux_courts_facility_name")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.FacilityId, entity.Status, entity.Name }, "ix_courts_facility_status_name")
            .HasDatabaseName("ix_courts_facility_status_name")
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.ScheduleTemplateId, entity.Id }, "ix_courts_schedule_template")
            .HasDatabaseName("ix_courts_schedule_template")
            .HasFilter("schedule_template_id IS NOT NULL AND is_deleted = false");
    }
}
