using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class FinancialTransactionConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        EntityConfiguration.Configure(builder, "financial_transactions");

        builder.Property(entity => entity.BookingId)
            .HasColumnName("booking_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.OwnerProfileId)
            .HasColumnName("owner_profile_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.DestinationBankAccountId)
            .HasColumnName("destination_bank_account_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.ParentTransactionId)
            .HasColumnName("parent_transaction_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.Type)
            .HasColumnName("type")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasConversion(new EnumStorageConverter<FinancialTransactionType>())
            .IsRequired();

        builder.Property(entity => entity.Amount)
            .HasColumnName("amount")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(entity => entity.Provider)
            .HasColumnName("provider")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);

        builder.Property(entity => entity.ProviderReference)
            .HasColumnName("provider_reference")
            .HasColumnType("varchar(255)")
            .HasMaxLength(255);

        builder.HasOne(entity => entity.Booking)
            .WithMany()
            .HasForeignKey(entity => entity.BookingId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_financial_transactions_booking_id");

        builder.HasOne(entity => entity.OwnerProfile)
            .WithMany()
            .HasForeignKey(entity => entity.OwnerProfileId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_financial_transactions_owner_profile_id");

        builder.HasOne(entity => entity.DestinationBankAccount)
            .WithMany()
            .HasForeignKey(entity => entity.DestinationBankAccountId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_financial_transactions_destination_bank_account_id");

        builder.HasOne(entity => entity.ParentTransaction)
            .WithMany()
            .HasForeignKey(entity => entity.ParentTransactionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_financial_transactions_parent_transaction_id");

        builder.ToTable("financial_transactions", table =>
        {
            table.HasCheckConstraint("ck_financial_transactions_type",
                "type IN ('PAYMENT', 'REFUND', 'PAYOUT')");
            table.HasCheckConstraint("ck_financial_transactions_amount",
                "amount > 0");
            table.HasCheckConstraint("ck_financial_transactions_provider",
                "(provider IS NULL) = (provider_reference IS NULL)");
            table.HasCheckConstraint("ck_financial_transactions_no_self_parent",
                "parent_transaction_id IS NULL OR parent_transaction_id <> id");
            table.HasCheckConstraint("ck_financial_transactions_target",
                "(type = 'PAYMENT' AND booking_id IS NOT NULL AND owner_profile_id IS NULL AND destination_bank_account_id IS NULL AND parent_transaction_id IS NULL) OR (type = 'REFUND' AND booking_id IS NOT NULL AND parent_transaction_id IS NOT NULL AND destination_bank_account_id IS NOT NULL AND owner_profile_id IS NULL) OR (type = 'PAYOUT' AND owner_profile_id IS NOT NULL AND destination_bank_account_id IS NOT NULL AND booking_id IS NULL AND parent_transaction_id IS NULL)");
        });

        builder.HasIndex(entity => new { entity.Provider, entity.ProviderReference }, "ux_financial_transactions_provider_reference")
            .HasDatabaseName("ux_financial_transactions_provider_reference")
            .IsUnique()
            .HasFilter("provider_reference IS NOT NULL");

        builder.HasIndex(entity => new { entity.BookingId, entity.Type, entity.CreatedAt }, "ix_financial_transactions_booking_type_created_at")
            .HasDatabaseName("ix_financial_transactions_booking_type_created_at")
            .IsDescending(false, false, true)
            .HasFilter("booking_id IS NOT NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.OwnerProfileId, entity.Type, entity.CreatedAt }, "ix_financial_transactions_owner_type_created_at")
            .HasDatabaseName("ix_financial_transactions_owner_type_created_at")
            .IsDescending(false, false, true)
            .HasFilter("owner_profile_id IS NOT NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.DestinationBankAccountId, entity.CreatedAt }, "ix_financial_transactions_destination_bank_account")
            .HasDatabaseName("ix_financial_transactions_destination_bank_account")
            .IsDescending(false, true)
            .HasFilter("destination_bank_account_id IS NOT NULL");

        builder.HasIndex(entity => entity.ParentTransactionId, "ix_financial_transactions_parent")
            .HasDatabaseName("ix_financial_transactions_parent")
            .HasFilter("parent_transaction_id IS NOT NULL");

        builder.HasIndex(entity => new { entity.Type, entity.CreatedAt }, "ix_financial_transactions_type_created_at")
            .HasDatabaseName("ix_financial_transactions_type_created_at")
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");
    }
}
