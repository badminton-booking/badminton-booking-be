using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.facility;

public sealed class ScheduleTemplateConfiguration : BaseEntityConfiguration<ScheduleTemplate>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ScheduleTemplate> builder)
    {
        builder.ToTable("schedule_templates", table =>
        {
            table.HasCheckConstraint("ck_schedule_templates_version", "version > 0");
            table.HasCheckConstraint("ck_schedule_templates_duration", "slot_duration_minutes BETWEEN 15 AND 720");
        });

        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(1000);

        // Cùng 1 cơ sở không có 2 template trùng (tên + phiên bản)
        builder.HasNamedIndex(x => new { x.FacilityId, x.Name, x.Version }, "ux_schedule_templates_facility_name_version")
            .UniqueWhenActive();

        builder.HasOne(x => x.Facility)
            .WithMany(facility => facility.ScheduleTemplates)
            .HasForeignKey(x => x.FacilityId);
    }
}
