using be_rallyhubv2.infrastructure.passwordHasher;
using Microsoft.Extensions.Options;

namespace be_rallyhubv2.tests.unit.infrastructure;

public class Pbkdf2PasswordHasherTests
{
    private static Pbkdf2PasswordHasher Create(string pepper = "pepper-pepper-pepper", int iterations = 10_000)
        => new(Options.Create(new PasswordHashingOptions { Pepper = pepper, Iterations = iterations }));

    [Fact]
    public void Correct_password_verifies()
    {
        var hasher = Create();

        var hash = hasher.Hash("S3cret-pass");

        Assert.True(hasher.Verify("S3cret-pass", hash));
    }

    [Fact]
    public void Wrong_password_does_not_verify()
    {
        var hasher = Create();

        var hash = hasher.Hash("S3cret-pass");

        Assert.False(hasher.Verify("wrong-pass", hash));
    }

    [Fact]
    public void Same_password_hashes_differently_each_time()
    {
        var hasher = Create();

        Assert.NotEqual(hasher.Hash("S3cret-pass"), hasher.Hash("S3cret-pass"));
    }

    [Fact]
    public void Different_pepper_does_not_verify()
    {
        var hash = Create("pepper-one-pepper-one").Hash("S3cret-pass");

        Assert.False(Create("pepper-two-pepper-two").Verify("S3cret-pass", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("PBKDF2-SHA256$abc$AAAA$AAAA")]
    [InlineData("PBKDF2-SHA256$10000$!!!$!!!")]
    public void Malformed_hash_does_not_verify(string badHash)
    {
        Assert.False(Create().Verify("S3cret-pass", badHash));
    }

    [Fact]
    public void Hash_made_with_old_iteration_count_still_verifies()
    {
        var oldHash = Create(iterations: 10_000).Hash("S3cret-pass");

        Assert.True(Create(iterations: 20_000).Verify("S3cret-pass", oldHash));
    }

    [Fact]
    public void Hash_fits_the_password_hash_column()
    {
        Assert.True(Create().Hash("S3cret-pass").Length <= 512);
    }
}