using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class UserBankAccountConfiguration : IEntityTypeConfiguration<UserBankAccount>
{
    public void Configure(EntityTypeBuilder<UserBankAccount> builder)
    {
        EntityConfiguration.Configure(builder, "user_bank_accounts");

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.BankCode)
            .HasColumnName("bank_code")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(entity => entity.AccountNumberEncrypted)
            .HasColumnName("account_number_encrypted")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(entity => entity.AccountNumberHash)
            .HasColumnName("account_number_hash")
            .HasColumnType("char(64)")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(entity => entity.AccountHolderName)
            .HasColumnName("account_holder_name")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(entity => entity.IsDefault)
            .HasColumnName("is_default")
            .HasColumnType("boolean")
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_user_bank_accounts_user_id");

        builder.ToTable("user_bank_accounts", table =>
        {
            table.HasCheckConstraint("ck_user_bank_accounts_bank_code",
                "bank_code = upper(btrim(bank_code)) AND length(bank_code) > 0");
            table.HasCheckConstraint("ck_user_bank_accounts_number_encrypted",
                "length(btrim(account_number_encrypted)) > 0");
            table.HasCheckConstraint("ck_user_bank_accounts_number_hash",
                "account_number_hash ~ '^[0-9a-f]{64}$'");
            table.HasCheckConstraint("ck_user_bank_accounts_holder_name",
                "length(btrim(account_holder_name)) > 0");
        });

        builder.HasIndex(entity => new { entity.UserId, entity.BankCode, entity.AccountNumberHash }, "ux_user_bank_accounts_user_bank_number_active")
            .HasDatabaseName("ux_user_bank_accounts_user_bank_number_active")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => entity.UserId, "ux_user_bank_accounts_one_default_per_user")
            .HasDatabaseName("ux_user_bank_accounts_one_default_per_user")
            .IsUnique()
            .HasFilter("is_default = true AND is_deleted = false");

        builder.HasIndex(entity => new { entity.UserId, entity.IsDefault, entity.CreatedAt }, "ix_user_bank_accounts_user_active")
            .HasDatabaseName("ix_user_bank_accounts_user_active")
            .IsDescending(false, true, true)
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.BankCode, entity.AccountNumberHash }, "ix_user_bank_accounts_bank_number_hash")
            .HasDatabaseName("ix_user_bank_accounts_bank_number_hash")
            .HasFilter("is_deleted = false");
    }
}
