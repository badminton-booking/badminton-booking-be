using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class LedgerAccountConfiguration : IEntityTypeConfiguration<LedgerAccount>
{
    public void Configure(EntityTypeBuilder<LedgerAccount> builder)
    {
        EntityConfiguration.Configure(builder, "ledger_accounts");

        builder.Property(entity => entity.OwnerProfileId)
            .HasColumnName("owner_profile_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.AccountType)
            .HasColumnName("account_type")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .HasConversion(new EnumStorageConverter<LedgerAccountType>())
            .IsRequired();

        builder.Property(entity => entity.Balance)
            .HasColumnName("balance")
            .HasColumnType("bigint")
            .IsRequired()
            .HasDefaultValue(0L);

        builder.HasOne(entity => entity.OwnerProfile)
            .WithMany()
            .HasForeignKey(entity => entity.OwnerProfileId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_ledger_accounts_owner_profile_id");

        builder.ToTable("ledger_accounts", table =>
        {
            table.HasCheckConstraint("ck_ledger_accounts_type",
                "account_type IN ('PLATFORM_CASH', 'PLATFORM_REVENUE', 'OWNER_PAYABLE')");
            table.HasCheckConstraint("ck_ledger_accounts_owner",
                "(account_type = 'OWNER_PAYABLE' AND owner_profile_id IS NOT NULL) OR (account_type IN ('PLATFORM_CASH', 'PLATFORM_REVENUE') AND owner_profile_id IS NULL)");
        });

        builder.HasIndex(entity => entity.AccountType, "ux_ledger_accounts_platform_type")
            .HasDatabaseName("ux_ledger_accounts_platform_type")
            .IsUnique()
            .HasFilter("owner_profile_id IS NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.OwnerProfileId, entity.AccountType }, "ux_ledger_accounts_owner_type")
            .HasDatabaseName("ux_ledger_accounts_owner_type")
            .IsUnique()
            .HasFilter("owner_profile_id IS NOT NULL AND is_deleted = false");

        builder.HasIndex(entity => entity.OwnerProfileId, "ix_ledger_accounts_owner_profile_id")
            .HasDatabaseName("ix_ledger_accounts_owner_profile_id")
            .HasFilter("owner_profile_id IS NOT NULL AND is_deleted = false");
    }
}
