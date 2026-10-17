using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        EntityConfiguration.Configure(builder, "refresh_tokens");

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("char(64)")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(entity => entity.FamilyId)
            .HasColumnName("family_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(entity => entity.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz");

        builder.Property(entity => entity.ReplacedByTokenId)
            .HasColumnName("replaced_by_token_id")
            .HasColumnType("uuid");

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_refresh_tokens_user_id");

        builder.HasOne(entity => entity.ReplacedByToken)
            .WithOne()
            .HasForeignKey<RefreshToken>(entity => entity.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_refresh_tokens_replaced_by_token_id");

        builder.ToTable("refresh_tokens", table =>
        {
            table.HasCheckConstraint("ck_refresh_tokens_expiration",
                "expires_at > created_at");
            table.HasCheckConstraint("ck_refresh_tokens_revoked_at",
                "revoked_at IS NULL OR revoked_at >= created_at");
            table.HasCheckConstraint("ck_refresh_tokens_replacement",
                "replaced_by_token_id IS NULL OR revoked_at IS NOT NULL");
            table.HasCheckConstraint("ck_refresh_tokens_no_self_reference",
                "replaced_by_token_id IS NULL OR replaced_by_token_id <> id");
        });

        builder.HasIndex(entity => entity.TokenHash, "ux_refresh_tokens_token_hash")
            .HasDatabaseName("ux_refresh_tokens_token_hash")
            .IsUnique();

        builder.HasIndex(entity => entity.ReplacedByTokenId, "ux_refresh_tokens_replaced_by_token_id")
            .HasDatabaseName("ux_refresh_tokens_replaced_by_token_id")
            .IsUnique()
            .HasFilter("replaced_by_token_id IS NOT NULL");

        builder.HasIndex(entity => new { entity.UserId, entity.ExpiresAt }, "ix_refresh_tokens_user_active")
            .HasDatabaseName("ix_refresh_tokens_user_active")
            .HasFilter("revoked_at IS NULL AND is_deleted = false");

        builder.HasIndex(entity => entity.FamilyId, "ix_refresh_tokens_family_active")
            .HasDatabaseName("ix_refresh_tokens_family_active")
            .HasFilter("is_deleted = false");
    }
}
