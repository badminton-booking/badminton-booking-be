using System.ComponentModel.DataAnnotations;

namespace be_rallyhubv2.infrastructure.authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;//ai phát hành

    [Required]
    public string Audience { get; init; } = string.Empty;//dành cho ai

    [Required, MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; init; } = 15;

    [Range(1, 365)]
    public int RefreshTokenDays { get; init; } = 14;
}