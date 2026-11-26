using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.infrastructure.mail;
using Microsoft.Extensions.Options;
using MimeKit;

namespace be_rallyhubv2.tests.unit.infrastructure;

public class SmtpEmailSenderTests
{
    private static SmtpEmailSender Create() => new(Options.Create(new MailOptions
    {
        Host = "localhost",
        FromAddress = "no-reply@rallyhub.test",
        DisplayName = "RallyHub"
    }));

    [Fact]
    public void Message_has_sender_recipient_subject_and_html_body()
    {
        var mime = Create().BuildMessage(new EmailMessage("customer@example.com", "Booking confirmation", "<p>Hello</p>"));

        Assert.Equal("no-reply@rallyhub.test", ((MailboxAddress)mime.From.Single()).Address);
        Assert.Equal("RallyHub", ((MailboxAddress)mime.From.Single()).Name);
        Assert.Equal("customer@example.com", ((MailboxAddress)mime.To.Single()).Address);
        Assert.Equal("Booking confirmation", mime.Subject);
        Assert.Equal("<p>Hello</p>", mime.HtmlBody);
    }

    [Fact]
    public void Invalid_recipient_throws()
    {
        Assert.ThrowsAny<Exception>(() => Create().BuildMessage(new EmailMessage("not an email", "Subject", "<p>x</p>")));
    }
}