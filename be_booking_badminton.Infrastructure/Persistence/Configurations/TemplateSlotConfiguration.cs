using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class TemplateSlotConfiguration : IEntityTypeConfiguration<TemplateSlot>
{
    public void Configure(EntityTypeBuilder<TemplateSlot> builder)
    {
        EntityConfiguration.Configure(builder, "template_slots");

        builder.Property(entity => entity.ScheduleTemplateId)
            .HasColumnName("schedule_template_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.DayOfWeek)
            .HasColumnName("day_of_week")
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(entity => entity.StartTime)
            .HasColumnName("start_time")
            .HasColumnType("time")
            .IsRequired();

        builder.Property(entity => entity.EndTime)
            .HasColumnName("end_time")
            .HasColumnType("time")
            .IsRequired();

        builder.Property(entity => entity.Price)
            .HasColumnName("price")
            .HasColumnType("bigint")
            .IsRequired();

        builder.HasOne(entity => entity.ScheduleTemplate)
            .WithMany()
            .HasForeignKey(entity => entity.ScheduleTemplateId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_template_slots_schedule_template_id");

        builder.ToTable("template_slots", table =>
        {
            table.HasCheckConstraint("ck_template_slots_day",
                "day_of_week BETWEEN 1 AND 7");
            table.HasCheckConstraint("ck_template_slots_time",
                "end_time > start_time");
            table.HasCheckConstraint("ck_template_slots_price",
                "price >= 0");
        });

        builder.HasIndex(entity => new { entity.ScheduleTemplateId, entity.DayOfWeek, entity.StartTime }, "ux_template_slots_template_day_start")
            .HasDatabaseName("ux_template_slots_template_day_start")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.ScheduleTemplateId, entity.DayOfWeek, entity.StartTime, entity.EndTime }, "ix_template_slots_template_day_time")
            .HasDatabaseName("ix_template_slots_template_day_time")
            .IncludeProperties("Price")
            .HasFilter("is_deleted = false");
    }
}
