using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using be_rallyhubv2.infrastructure.authentication;
using Microsoft.IdentityModel.Tokens;

namespace be_rallyhubv2.tests.unit.infrastructure;

public class JwtTokenServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "rallyhub-test",
        Audience = "rallyhub-test-clients",
        SigningKey = "0123456789-0123456789-0123456789-ab",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 14
    };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static JwtTokenService Create(TimeProvider? time = null)
        => new(Microsoft.Extensions.Options.Options.Create(Options), time ?? TimeProvider.System);

    private static ClaimsPrincipal Validate(string token, JwtOptions? options = null)
        => new JwtSecurityTokenHandler().ValidateToken(
            token, JwtValidation.CreateParameters(options ?? Options), out _);

    [Fact]
    public void Access_token_round_trips_user_id_and_roles()
    {
        var userId = Guid.NewGuid();

        var token = Create().CreateAccessToken(userId, new[] { "OWNER", "STAFF" });
        var principal = Validate(token);

        Assert.Equal(userId.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Assert.Equal(new[] { "OWNER", "STAFF" }, principal.FindAll(ClaimTypes.Role).Select(c => c.Value).Order());
    }

    [Fact]
    public void Expired_access_token_is_rejected()
    {
        var twoHoursAgo = new FixedTimeProvider(DateTimeOffset.UtcNow.AddHours(-2));

        var token = Create(twoHoursAgo).CreateAccessToken(Guid.NewGuid(), new[] { "OWNER" });

        Assert.Throws<SecurityTokenExpiredException>(() => Validate(token));
    }

    [Fact]
    public void Token_signed_with_another_key_is_rejected()
    {
        var token = Create().CreateAccessToken(Guid.NewGuid(), new[] { "OWNER" });
        var otherKey = new JwtOptions
        {
            Issuer = Options.Issuer,
            Audience = Options.Audience,
            SigningKey = "another-key-another-key-another-key-xx"
        };

        Assert.ThrowsAny<SecurityTokenException>(() => Validate(token, otherKey));
    }

    [Fact]
    public void Refresh_tokens_are_random_and_long_enough()
    {
        var service = Create();

        var first = service.CreateRefreshToken();
        var second = service.CreateRefreshToken();

        Assert.NotEqual(first, second);
        Assert.True(first.Length >= 80);
    }

    [Fact]
    public void Token_hash_is_a_64_char_lowercase_hex_and_deterministic()
    {
        var service = Create();

        var hash = service.HashToken("some-token");

        Assert.Matches("^[0-9a-f]{64}$", hash);
        Assert.Equal(hash, service.HashToken("some-token"));
        Assert.NotEqual(hash, service.HashToken("other-token"));
    }

    [Fact]
    public void Lifetimes_come_from_options()
    {
        var service = Create();

        Assert.Equal(TimeSpan.FromMinutes(15), service.AccessTokenLifetime);
        Assert.Equal(TimeSpan.FromDays(14), service.RefreshTokenLifetime);
    }
}