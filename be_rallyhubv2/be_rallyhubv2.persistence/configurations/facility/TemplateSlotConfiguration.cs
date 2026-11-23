using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.facility;

public sealed class TemplateSlotConfiguration : BaseEntityConfiguration<TemplateSlot>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TemplateSlot> builder)
    {
        builder.ToTable("template_slots", table =>
        {
            table.HasCheckConstraint("ck_template_slots_day", "day_of_week BETWEEN 1 AND 7");
            table.HasCheckConstraint("ck_template_slots_time", "end_time > start_time");
            table.HasCheckConstraint("ck_template_slots_price", "price >= 0");
        });

        // Trong 1 template, mỗi thứ chỉ có 1 slot bắt đầu tại 1 giờ
        builder.HasNamedIndex(x => new { x.ScheduleTemplateId, x.DayOfWeek, x.StartTime }, "ux_template_slots_template_day_start")
            .UniqueWhenActive();

        builder.HasOne(x => x.ScheduleTemplate)
            .WithMany(template => template.TemplateSlots)
            .HasForeignKey(x => x.ScheduleTemplateId);
    }   
}
