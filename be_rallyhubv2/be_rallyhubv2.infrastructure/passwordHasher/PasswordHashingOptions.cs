using System.ComponentModel.DataAnnotations;

namespace be_rallyhubv2.infrastructure.passwordHasher;

public sealed class PasswordHashingOptions
{
    public const string SectionName = "PasswordHashing";

    [Required, MinLength(16)]
    public string Pepper { get; init; } = string.Empty;

    [Range(10_000, 5_000_000)]
    public int Iterations { get; init; } = 600_000;
}