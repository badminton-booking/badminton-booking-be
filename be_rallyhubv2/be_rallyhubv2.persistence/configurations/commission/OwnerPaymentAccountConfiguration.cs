using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.commission;

public sealed class OwnerPaymentAccountConfiguration : BaseEntityConfiguration<OwnerPaymentAccount>
{
    protected override void ConfigureEntity(EntityTypeBuilder<OwnerPaymentAccount> builder)
    {
        builder.ToTable("owner_payment_accounts");

        builder.Property(x => x.Provider).IsRequired().HasMaxLength(50).HasDefaultValue("SEPAY");
        builder.Property(x => x.AccountNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.AccountHolder).IsRequired().HasMaxLength(150);
        builder.Property(x => x.BankCode).HasMaxLength(50);
        builder.Property(x => x.AccountRef).IsRequired().HasMaxLength(255);

        // Mỗi Owner chỉ có 1 tài khoản nhận tiền đang hoạt động
        builder.HasNamedIndex(x => x.OwnerProfileId, "ux_owner_payment_accounts_active")
            .UniqueWhenActive("is_active = true");

        // 1 số tài khoản chỉ thuộc về 1 Owner
        builder.HasNamedIndex(x => new { x.Provider, x.AccountNumber }, "ux_owner_payment_accounts_number")
            .UniqueWhenActive();

        builder.HasOne(x => x.OwnerProfile)
            .WithMany(owner => owner.PaymentAccounts)
            .HasForeignKey(x => x.OwnerProfileId);
    }
}
