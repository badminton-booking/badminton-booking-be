using be_booking_badminton.Domain.Common;

namespace be_booking_badminton.Domain.Entities;

public class DirectMessage : BaseEntity<Guid>
{
    public Guid ConversationId { get; set; }
    public DirectConversation? Conversation { get; set; }
    public Guid SenderUserId { get; set; }
    public User? SenderUser { get; set; }
    /// <summary>ID do client tạo cho một tin; giữ nguyên khi gửi lại để tránh lưu trùng.</summary>
    public Guid ClientMessageId { get; set; }
    /// <summary>Số thứ tự của tin trong cuộc chat riêng, do server cấp.</summary>
    public long Sequence { get; set; }
    public string? Content { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
