using System.Text.Json;
using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.infrastructure.mail;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Options;

namespace be_rallyhubv2.tests.integration.infrastructure;

public sealed class MailpitFixture : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder()
        .WithImage("axllent/mailpit:latest")
        .WithPortBinding(1025, true)
        .WithPortBinding(8025, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request.ForPort(8025)))
        .Build();

    public string SmtpHost => _container.Hostname;
    public int SmtpPort => _container.GetMappedPublicPort(1025);
    public Uri ApiBase => new($"http://{_container.Hostname}:{_container.GetMappedPublicPort(8025)}");

    public Task InitializeAsync() => _container.StartAsync();

    public async Task DisposeAsync() => await _container.DisposeAsync();
}

public class SmtpEmailSenderMailpitTests(MailpitFixture fixture) : IClassFixture<MailpitFixture>
{
    [Fact]
    public async Task Email_is_delivered_to_the_smtp_server()
    {
        var sender = new SmtpEmailSender(Options.Create(new MailOptions
        {
            Host = fixture.SmtpHost,
            Port = fixture.SmtpPort,
            UseSsl = false,
            FromAddress = "no-reply@rallyhub.test",
            DisplayName = "RallyHub"
        }));
        var subject = "Booking confirmation " + Guid.NewGuid().ToString("N");

        await sender.SendAsync(new EmailMessage("customer@example.com", subject, "<p>Hello</p>"), default);

        using var http = new HttpClient();
        var json = await http.GetStringAsync(new Uri(fixture.ApiBase, "/api/v1/messages"));
        using var document = JsonDocument.Parse(json);
        var messages = document.RootElement.GetProperty("messages").EnumerateArray().ToList();

        var delivered = Assert.Single(messages, m => m.GetProperty("Subject").GetString() == subject);
        Assert.Equal("customer@example.com", delivered.GetProperty("To")[0].GetProperty("Address").GetString());
        Assert.Equal("no-reply@rallyhub.test", delivered.GetProperty("From").GetProperty("Address").GetString());
    }
}