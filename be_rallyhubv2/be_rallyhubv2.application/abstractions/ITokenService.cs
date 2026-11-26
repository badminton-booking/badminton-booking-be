namespace be_rallyhubv2.application.abstractions;

public interface ITokenService
{
    TimeSpan AccessTokenLifetime { get; }
    TimeSpan RefreshTokenLifetime { get; }

    string CreateAccessToken(Guid userId, IEnumerable<string> roles);
    string CreateRefreshToken();
    string HashToken(string token);
}