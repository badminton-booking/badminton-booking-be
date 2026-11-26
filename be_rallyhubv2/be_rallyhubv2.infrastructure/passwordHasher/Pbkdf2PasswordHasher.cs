using System.Security.Cryptography;
using be_rallyhubv2.application.abstractions;
using Microsoft.Extensions.Options;

namespace be_rallyhubv2.infrastructure.passwordHasher;

public sealed class Pbkdf2PasswordHasher(IOptions<PasswordHashingOptions> options) : IPasswordHasher
{

    private const string Prefix = "PBKDF2-SHA256";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    
    private readonly PasswordHashingOptions _options = options.Value;
    
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(password, salt, _options.Iterations, HashSize);

        return $"{Prefix}${_options.Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out var iterations))
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual =  Derive(password, salt, iterations, expected.Length);
            
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch(Exception)
        {
            return false;
        }
    }
    
    private byte[] Derive(string password, byte[] salt, int iterations, int length)
    => Rfc2898DeriveBytes.Pbkdf2(password + _options.Pepper, salt, iterations, HashAlgorithmName.SHA256, length);
}