using be_rallyhubv2.infrastructure.dependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace be_rallyhubv2.tests.unit.infrastructure;

public class StartupValidationTests
{
    private static IHost BuildHost(Dictionary<string, string?> settings)
        => Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(settings))
            .ConfigureServices((context, services) => services.AddInfrastructure(context.Configuration))
            .Build();

    private static Dictionary<string, string?> Settings(string signingKey) => new()
    {
        ["Jwt:Issuer"] = "rallyhub",
        ["Jwt:Audience"] = "rallyhub-clients",
        ["Jwt:SigningKey"] = signingKey,
        ["PasswordHashing:Pepper"] = "pepper-pepper-pepper",
        ["Mail:Host"] = "localhost",
        ["Mail:FromAddress"] = "no-reply@rallyhub.test",
        ["Mail:DisplayName"] = "RallyHub",
        ["Cloudinary:CloudName"] = "demo",
        ["Cloudinary:ApiKey"] = "key",
        ["Cloudinary:ApiSecret"] = "secret",
        ["ConnectionStrings:Redis"] = "localhost:6379"
    };

    [Fact]
    public async Task Invalid_configuration_stops_the_host_from_starting()
    {
        using var host = BuildHost(Settings(signingKey: "too-short"));

        await Assert.ThrowsAsync<OptionsValidationException>(() => host.StartAsync());
    }

    [Fact]
    public async Task Valid_configuration_lets_the_host_start()
    {
        using var host = BuildHost(Settings(signingKey: "0123456789-0123456789-0123456789-ab"));

        await host.StartAsync();
        await host.StopAsync();
    }
}