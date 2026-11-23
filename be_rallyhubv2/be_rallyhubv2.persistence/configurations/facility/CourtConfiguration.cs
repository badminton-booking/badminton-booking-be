using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.facility;

public sealed class CourtConfiguration : BaseEntityConfiguration<Court>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Court> builder)
    {
        builder.ToTable("courts");

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(1000);

        // Trong 1 cơ sở không có 2 sân trùng tên
        builder.HasNamedIndex(x => new { x.FacilityId, x.Name }, "ux_courts_facility_name")
            .UniqueWhenActive();

        builder.HasOne(x => x.Facility)
            .WithMany(facility => facility.Courts)
            .HasForeignKey(x => x.FacilityId);

        // Sân có thể chưa gán template (nullable)
        builder.HasOne(x => x.ScheduleTemplate)
            .WithMany()
            .HasForeignKey(x => x.ScheduleTemplateId);
    }
}
