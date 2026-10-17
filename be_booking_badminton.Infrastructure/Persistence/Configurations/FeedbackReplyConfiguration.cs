using be_booking_badminton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_booking_badminton.Infrastructure.Persistence.Configurations;

public sealed class FeedbackReplyConfiguration : IEntityTypeConfiguration<FeedbackReply>
{
    public void Configure(EntityTypeBuilder<FeedbackReply> builder)
    {
        EntityConfiguration.Configure(builder, "feedback_replies");

        builder.Property(entity => entity.FeedbackId)
            .HasColumnName("feedback_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(entity => entity.ParentReplyId)
            .HasColumnName("parent_reply_id")
            .HasColumnType("uuid");

        builder.Property(entity => entity.Content)
            .HasColumnName("content")
            .HasColumnType("varchar(2000)")
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasOne(entity => entity.Feedback)
            .WithMany()
            .HasForeignKey(entity => entity.FeedbackId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_feedback_replies_feedback_id");

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_feedback_replies_user_id");

        builder.HasOne(entity => entity.ParentReply)
            .WithMany()
            .HasForeignKey(entity => entity.ParentReplyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_feedback_replies_parent_reply_id");

        builder.ToTable("feedback_replies", table =>
        {
            table.HasCheckConstraint("ck_feedback_replies_content",
                "length(btrim(content)) > 0");
            table.HasCheckConstraint("ck_feedback_replies_no_self_parent",
                "parent_reply_id IS NULL OR parent_reply_id <> id");
        });

        builder.HasIndex(entity => new { entity.FeedbackId, entity.ParentReplyId, entity.CreatedAt, entity.Id }, "ix_feedback_replies_feedback_parent_created_at")
            .HasDatabaseName("ix_feedback_replies_feedback_parent_created_at")
            .HasFilter("is_deleted = false");

        builder.HasIndex(entity => new { entity.ParentReplyId, entity.CreatedAt, entity.Id }, "ix_feedback_replies_parent_created_at")
            .HasDatabaseName("ix_feedback_replies_parent_created_at")
            .HasFilter("parent_reply_id IS NOT NULL AND is_deleted = false");

        builder.HasIndex(entity => new { entity.UserId, entity.CreatedAt }, "ix_feedback_replies_user_created_at")
            .HasDatabaseName("ix_feedback_replies_user_created_at")
            .IsDescending(false, true)
            .HasFilter("is_deleted = false");
    }
}
