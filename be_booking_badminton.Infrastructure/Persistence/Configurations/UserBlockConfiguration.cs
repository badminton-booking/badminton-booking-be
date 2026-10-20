using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        builder.ToTable("UserBlocks", table =>
            // Một user không thể chặn chính mình.
            table.HasCheckConstraint("CKUserBlockDistinctUsers", "\"BlockerUserId\" <> \"BlockedUserId\""));
        EntityConfiguration.ConfigureBase(builder);

        builder.HasOne(block => block.BlockerUser)
            .WithMany()
            .HasForeignKey(block => block.BlockerUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(block => block.BlockedUser)
            .WithMany()
            .HasForeignKey(block => block.BlockedUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(block => new { block.BlockerUserId, block.BlockedUserId })
            .IsUnique()
            .HasDatabaseName("UXUserBlockPair");
        builder.HasIndex(block => new { block.BlockedUserId, block.BlockerUserId })
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IXUserBlockReverseActive");
    }
}
