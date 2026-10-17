namespace be_booking_badminton.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; }

    void MarkDeleted();
}
