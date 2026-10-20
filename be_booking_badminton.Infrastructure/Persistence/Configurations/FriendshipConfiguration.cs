using be_booking_badminton.Domain.Entities;
using be_booking_badminton.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships", table =>
        {
            // Một user không thể kết bạn với chính mình.
            table.HasCheckConstraint("CKFriendshipDistinctUsers", "\"RequesterUserId\" <> \"RecipientUserId\"");
            // Lời mời đang chờ thì chưa có RespondedAt; đã phản hồi thì bắt buộc có RespondedAt.
            table.HasCheckConstraint("CKFriendshipResponse", "(\"Status\" = 'pending' AND \"RespondedAt\" IS NULL) OR (\"Status\" <> 'pending' AND \"RespondedAt\" IS NOT NULL)");
            // Chỉ cho phép các trạng thái đã định nghĩa trong FriendshipStatus.
            table.HasCheckConstraint("CKFriendshipStatus", "\"Status\" IN ('pending', 'accepted', 'rejected', 'cancelled')");
        });
        EntityConfiguration.ConfigureBase(builder);

        builder.Property(friendship => friendship.Status)
            .HasConversion(status => status.Name, name => FriendshipStatus.FromName(name, false))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(friendship => friendship.RespondedAt).HasColumnType("timestamp with time zone");
        builder.Property(friendship => friendship.LastActivityAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        builder.HasOne(friendship => friendship.RequesterUser)
            .WithMany()
            .HasForeignKey(friendship => friendship.RequesterUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(friendship => friendship.RecipientUser)
            .WithMany()
            .HasForeignKey(friendship => friendship.RecipientUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(friendship => new { friendship.RequesterUserId, friendship.Status, friendship.LastActivityAt, friendship.Id })
            .IsDescending(false, false, true, true)
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IXFriendshipRequesterActive");
        builder.HasIndex(friendship => new { friendship.RecipientUserId, friendship.Status, friendship.LastActivityAt, friendship.Id })
            .IsDescending(false, false, true, true)
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IXFriendshipRecipientActive");
    }
}
