namespace be_booking_badminton.Application.Common.Models;

public sealed record EmailMessage(string To, string Subject, string HtmlBody);
