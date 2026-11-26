using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.infrastructure.authentication;
using be_rallyhubv2.infrastructure.dependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace be_rallyhubv2.tests.unit.infrastructure;

public class InfrastructureRegistrationTests
{
    private static Dictionary<string, string?> ValidSettings() => new()
    {
        ["Jwt:Issuer"] = "rallyhub",
        ["Jwt:Audience"] = "rallyhub-clients",
        ["Jwt:SigningKey"] = "0123456789-0123456789-0123456789-ab",
        ["PasswordHashing:Pepper"] = "pepper-pepper-pepper",
        ["Mail:Host"] = "localhost",
        ["Mail:FromAddress"] = "no-reply@rallyhub.test",
        ["Mail:DisplayName"] = "RallyHub",
        ["Cloudinary:CloudName"] = "demo",
        ["Cloudinary:ApiKey"] = "key",
        ["Cloudinary:ApiSecret"] = "secret",
        ["ConnectionStrings:Redis"] = "localhost:6379"
    };

    private static ServiceProvider Build(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void All_infrastructure_services_can_be_resolved()
    {
        using var provider = Build(ValidSettings());

        Assert.NotNull(provider.GetRequiredService<ITokenService>());
        Assert.NotNull(provider.GetRequiredService<IPasswordHasher>());
        Assert.NotNull(provider.GetRequiredService<IEmailSender>());
        Assert.NotNull(provider.GetRequiredService<IFileStorage>());
        Assert.NotNull(provider.GetRequiredService<ICacheService>());
    }

    [Fact]
    public void Short_signing_key_fails_options_validation()
    {
        var settings = ValidSettings();
        settings["Jwt:SigningKey"] = "too-short";
        using var provider = Build(settings);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<JwtOptions>>().Value);
    }

    [Fact]
    public void Missing_redis_connection_string_throws_at_registration()
    {
        var settings = ValidSettings();
        settings.Remove("ConnectionStrings:Redis");

        Assert.Throws<InvalidOperationException>(() => Build(settings));
    }
}