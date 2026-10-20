using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", table =>
        {
            // Token phải hết hạn sau thời điểm tạo.
            table.HasCheckConstraint("CKRefreshTokenExpiration", "\"ExpiresAt\" > \"CreatedAt\"");
            // Thời điểm thu hồi, nếu có, không được trước thời điểm tạo.
            table.HasCheckConstraint("CKRefreshTokenRevokedAt", "\"RevokedAt\" IS NULL OR \"RevokedAt\" >= \"CreatedAt\"");
            // Token chỉ được thay thế bằng token khác sau khi đã bị thu hồi.
            table.HasCheckConstraint("CKRefreshTokenReplacement", "\"ReplacedByTokenId\" IS NULL OR \"RevokedAt\" IS NOT NULL");
            // Token không được tự thay thế chính nó.
            table.HasCheckConstraint("CKRefreshTokenNoSelfReference", "\"ReplacedByTokenId\" IS NULL OR \"ReplacedByTokenId\" <> \"Id\"");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(token => token.TokenHash).HasColumnType("char(64)").IsRequired();
        builder.Property(token => token.ExpiresAt).HasColumnType("timestamp with time zone");
        builder.Property(token => token.RevokedAt).HasColumnType("timestamp with time zone");

        builder.HasOne(token => token.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(token => token.ReplacedByToken)
            .WithOne()
            .HasForeignKey<RefreshToken>(token => token.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(token => token.TokenHash).IsUnique().HasDatabaseName("UXRefreshTokenTokenHash");
        builder.HasIndex(token => token.ReplacedByTokenId)
            .IsUnique()
            .HasFilter("\"ReplacedByTokenId\" IS NOT NULL")
            .HasDatabaseName("UXRefreshTokenReplacedByTokenId");
        builder.HasIndex(token => new { token.UserId, token.ExpiresAt })
            .HasFilter("\"RevokedAt\" IS NULL AND \"IsDeleted\" = false")
            .HasDatabaseName("IXRefreshTokenUserActive");
    }
}
