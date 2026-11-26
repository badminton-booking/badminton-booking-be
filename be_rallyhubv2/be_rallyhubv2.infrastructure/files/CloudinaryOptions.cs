using System.ComponentModel.DataAnnotations;

namespace be_rallyhubv2.infrastructure.files;

public sealed class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    [Required]
    public string CloudName { get; init; } = string.Empty;

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Required]
    public string ApiSecret { get; init; } = string.Empty;

    [Range(1, 100_000_000)]
    public long MaxFileSizeBytes { get; init; } = 5_000_000;

    [Required]
    public string FolderPrefix { get; init; } = "rallyhub";
}