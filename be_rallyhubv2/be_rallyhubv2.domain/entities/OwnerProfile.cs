using be_rallyhubv2.domain.common;

namespace be_rallyhubv2.domain.entities;

public class OwnerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ApprovedOwnerRequestId { get; set; }
    public string BusinessName { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }

    // ===== Relationships =====
    public User? User { get; set; }
    public OwnerRequest? ApprovedOwnerRequest { get; set; }
    public ICollection<Facility> Facilities { get; set; } = new List<Facility>();
    public ICollection<OwnerPaymentAccount> PaymentAccounts { get; set; } = new List<OwnerPaymentAccount>();
}
