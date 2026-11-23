using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.identity;

public sealed class RefreshTokenConfiguration : BaseEntityConfiguration<RefreshToken>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", table =>
        {
            table.HasCheckConstraint("ck_refresh_tokens_expiration", "expires_at > created_at");
            table.HasCheckConstraint(
                "ck_refresh_tokens_no_self_reference",
                "replaced_by_token_id IS NULL OR replaced_by_token_id <> id");
        });

        // SHA-256 dạng hex = đúng 64 ký tự
        builder.Property(x => x.TokenHash).IsRequired().HasColumnType("char(64)");

        builder.HasNamedIndex(x => x.TokenHash, "ux_refresh_tokens_token_hash").IsUnique();

        // Mỗi token chỉ bị thay thế bởi tối đa 1 token mới
        builder.HasNamedIndex(x => x.ReplacedByTokenId, "ux_refresh_tokens_replaced_by_token_id")
            .IsUnique().HasFilter("replaced_by_token_id IS NOT NULL");

        // Tìm các token còn hiệu lực của 1 user
        builder.HasNamedIndex(x => new { x.UserId, x.ExpiresAt }, "ix_refresh_tokens_user_active")
            .WhenActive("revoked_at IS NULL");

        // Thu hồi cả "họ" token khi phát hiện bị dùng lại
        builder.HasNamedIndex(x => x.FamilyId, "ix_refresh_tokens_family_active").WhenActive();

        builder.HasOne(x => x.User).WithMany(user => user.RefreshTokens).HasForeignKey(x => x.UserId);

        // Tự tham chiếu: token này được thay bằng token nào
        builder.HasOne(x => x.ReplacedByToken).WithOne().HasForeignKey<RefreshToken>(x => x.ReplacedByTokenId);
    }
}
