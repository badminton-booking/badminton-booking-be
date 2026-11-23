using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.identity;

public sealed class UserExternalLoginConfiguration : BaseEntityConfiguration<UserExternalLogin>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserExternalLogin> builder)
    {
        builder.ToTable("user_external_logins");

        builder.Property(x => x.ProviderSubject).IsRequired().HasMaxLength(255);

        // 1 tài khoản Google chỉ gắn với 1 User
        builder.HasNamedIndex(x => new { x.Provider, x.ProviderSubject }, "ux_user_external_logins_provider_subject")
            .UniqueWhenActive();

        // 1 User chỉ gắn 1 tài khoản cho mỗi provider
        builder.HasNamedIndex(x => new { x.UserId, x.Provider }, "ux_user_external_logins_user_provider")
            .UniqueWhenActive();

        builder.HasOne(x => x.User).WithMany(user => user.ExternalLogins).HasForeignKey(x => x.UserId);
    }
}
