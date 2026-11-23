using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.platform;

public sealed class ConfigConfiguration : BaseEntityConfiguration<Config>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Config> builder)
    {
        builder.ToTable("configs", table =>
        {
            // PLATFORM thì không có facility_id; FACILITY thì bắt buộc có
            table.HasCheckConstraint(
                "ck_configs_facility_scope",
                "(scope = 'PLATFORM' AND facility_id IS NULL) OR (scope = 'FACILITY' AND facility_id IS NOT NULL)");
        });

        builder.Property(x => x.Key).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Value).IsRequired().HasColumnType("jsonb");

        // Mỗi key của nền tảng chỉ có 1 giá trị
        builder.HasNamedIndex(x => x.Key, "ux_configs_platform_key")
            .UniqueWhenActive("scope = 'PLATFORM'");

        // Mỗi key của 1 cơ sở chỉ có 1 giá trị
        builder.HasNamedIndex(x => new { x.FacilityId, x.Key }, "ux_configs_facility_key")
            .UniqueWhenActive("scope = 'FACILITY'");

        builder.HasOne(x => x.Facility)
            .WithMany()
            .HasForeignKey(x => x.FacilityId);
    }
}
