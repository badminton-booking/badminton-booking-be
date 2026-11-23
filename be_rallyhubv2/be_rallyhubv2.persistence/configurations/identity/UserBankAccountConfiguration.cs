using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.identity;

public sealed class UserBankAccountConfiguration : BaseEntityConfiguration<UserBankAccount>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserBankAccount> builder)
    {
        builder.ToTable("user_bank_accounts", table =>
        {
            // bank_code và hash nằm trong unique index => mỗi giá trị chỉ được tồn tại ở 1 dạng chuẩn
            table.HasCheckConstraint(
                "ck_user_bank_accounts_bank_code",
                "bank_code = upper(btrim(bank_code)) AND length(bank_code) > 0");
            table.HasCheckConstraint("ck_user_bank_accounts_number_hash", "account_number_hash ~ '^[0-9a-f]{64}$'");
        });

        builder.Property(x => x.BankCode).IsRequired().HasMaxLength(20);
        builder.Property(x => x.AccountNumberEncrypted).IsRequired().HasColumnType("text");
        builder.Property(x => x.AccountNumberHash).IsRequired().HasColumnType("char(64)");
        builder.Property(x => x.AccountHolderName).IsRequired().HasMaxLength(150);

        // Không lưu trùng 1 tài khoản ngân hàng cho cùng 1 User (so sánh bằng hash)
        builder.HasNamedIndex(x => new { x.UserId, x.BankCode, x.AccountNumberHash }, "ux_user_bank_accounts_user_bank_number_active")
            .UniqueWhenActive();

        // Mỗi User chỉ có 1 tài khoản mặc định
        builder.HasNamedIndex(x => x.UserId, "ux_user_bank_accounts_one_default_per_user")
            .UniqueWhenActive("is_default = true");

        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
    }
}
