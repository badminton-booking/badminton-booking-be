using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.identity;

public sealed class OwnerProfileConfiguration : BaseEntityConfiguration<OwnerProfile>
{
    protected override void ConfigureEntity(EntityTypeBuilder<OwnerProfile> builder)
    {
        builder.ToTable("owner_profiles");

        builder.Property(x => x.BusinessName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.LogoUrl).HasMaxLength(2048);

        builder.HasNamedIndex(x => x.UserId, "ux_owner_profiles_user_id").IsUnique();
        builder.HasNamedIndex(x => x.ApprovedOwnerRequestId, "ux_owner_profiles_approved_request_id").IsUnique();

        builder.HasOne(x => x.User).WithOne(user => user.OwnerProfile).HasForeignKey<OwnerProfile>(x => x.UserId);
        builder.HasOne(x => x.ApprovedOwnerRequest).WithOne().HasForeignKey<OwnerProfile>(x => x.ApprovedOwnerRequestId);
    }
}
