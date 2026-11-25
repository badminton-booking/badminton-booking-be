namespace be_rallyhubv2.application.abstractions;

public interface ITokenService
{
    string CreateAccessToken(Guid userId, IEnumerable<string> roles);
    string CreateRefreshToken();
    string HashToken(string token);
}