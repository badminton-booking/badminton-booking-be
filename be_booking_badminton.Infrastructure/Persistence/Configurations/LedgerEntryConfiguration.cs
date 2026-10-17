using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        EntityConfiguration.Configure(builder, "ledger_entries");

        builder.Property(entity => entity.FinancialTransactionId)
            .HasColumnName("financial_transaction_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.LedgerAccountId)
            .HasColumnName("ledger_account_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.EntryType)
            .HasColumnName("entry_type")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .HasConversion(new EnumStorageConverter<LedgerEntryType>())
            .IsRequired();

        builder.Property(entity => entity.Amount)
            .HasColumnName("amount")
            .HasColumnType("bigint")
            .IsRequired();

        builder.HasOne(entity => entity.FinancialTransaction)
            .WithMany()
            .HasForeignKey(entity => entity.FinancialTransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_ledger_entries_financial_transaction_id");

        builder.HasOne(entity => entity.LedgerAccount)
            .WithMany()
            .HasForeignKey(entity => entity.LedgerAccountId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_ledger_entries_ledger_account_id");

        builder.ToTable("ledger_entries", table =>
        {
            table.HasCheckConstraint("ck_ledger_entries_type",
                "entry_type IN ('DEBIT', 'CREDIT')");
            table.HasCheckConstraint("ck_ledger_entries_amount",
                "amount > 0");
        });

        builder.HasIndex(entity => new { entity.FinancialTransactionId, entity.LedgerAccountId, entity.EntryType }, "ux_ledger_entries_transaction_account_type")
            .HasDatabaseName("ux_ledger_entries_transaction_account_type")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => entity.FinancialTransactionId, "ix_ledger_entries_transaction_id")
            .HasDatabaseName("ix_ledger_entries_transaction_id")
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.LedgerAccountId, entity.CreatedAt }, "ix_ledger_entries_account_created_at")
            .HasDatabaseName("ix_ledger_entries_account_created_at")
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");
    }
}
