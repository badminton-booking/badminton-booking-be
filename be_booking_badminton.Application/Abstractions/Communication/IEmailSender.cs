using be_booking_badminton.Application.Common.Models;

namespace be_booking_badminton.Application.Abstractions.Communication;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
