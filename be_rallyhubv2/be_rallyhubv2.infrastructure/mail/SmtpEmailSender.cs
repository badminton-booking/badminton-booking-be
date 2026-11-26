using be_rallyhubv2.application.abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;


namespace be_rallyhubv2.infrastructure.mail;

public sealed class SmtpEmailSender(IOptions<MailOptions> options): IEmailSender
{
    private readonly MailOptions _options = options.Value;
    
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var email = BuildMessage(message);
        
        using var client = new SmtpClient();
        
        var socketOptions = _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken);
        
        await client.SendAsync(email, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        
    }

    public MimeMessage BuildMessage(EmailMessage message)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_options.DisplayName, _options.FromAddress));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;
        email.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();

        return email;
    }
}