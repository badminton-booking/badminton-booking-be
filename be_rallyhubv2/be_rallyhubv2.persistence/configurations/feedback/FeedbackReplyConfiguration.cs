using be_rallyhubv2.domain.entities;
using be_rallyhubv2.persistence.configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace be_rallyhubv2.persistence.configurations.feedback;

public sealed class FeedbackReplyConfiguration : BaseEntityConfiguration<FeedbackReply>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FeedbackReply> builder)
    {
        builder.ToTable("feedback_replies");

        builder.Property(x => x.Content).IsRequired().HasMaxLength(2000);

        builder.HasOne(x => x.Feedback)
            .WithMany(feedback => feedback.Replies)
            .HasForeignKey(x => x.FeedbackId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}
