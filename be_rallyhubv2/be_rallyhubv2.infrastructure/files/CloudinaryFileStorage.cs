using be_rallyhubv2.application.abstractions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace be_rallyhubv2.infrastructure.files;

public sealed class CloudinaryFileStorage: IFileStorage
{
    
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };
    
    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _options;

    public CloudinaryFileStorage(IOptions<CloudinaryOptions> options)
    {
        _options = options.Value;
        _cloudinary = new Cloudinary(new Account(_options.CloudName, _options.ApiKey, _options.ApiSecret));
    }
    
    
    public async Task<StoredFile> UploadAsync(FileUpload upload, CancellationToken cancellationToken)
    {
        Validate(upload);
        
        var result = await _cloudinary.UploadAsync(
            new ImageUploadParams
            {
                File = new  FileDescription(upload.FileName, upload.Content),
                Folder = $"{_options.FolderPrefix}/{upload.Folder}"
            },
            cancellationToken);

        if (result.Error is not null)
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");

        return new StoredFile(result.SecureUrl.ToString(), result.PublicId);
        
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        var result = await _cloudinary.DestroyAsync(new DeletionParams(storageKey));
        
        if(result.Error is not null)
            throw new InvalidOperationException($"Cloudinary delete failed: {result.Error.Message}");
    }

    private void Validate(FileUpload upload)
    {
        if (!AllowedExtensions.Contains(Path.GetExtension(upload.FileName)))
            throw new ArgumentException("Only .jpg, .jpeg, .png and .webp files are allowed.", nameof(upload));

        if (!AllowedContentTypes.Contains(upload.ContentType))
            throw new ArgumentException("Only image/jpeg, image/png and image/webp are allowed.", nameof(upload));

        if (upload.Content.CanSeek && (upload.Content.Length == 0 || upload.Content.Length > _options.MaxFileSizeBytes))
            throw new ArgumentException("The file is empty or larger than the allowed size.", nameof(upload));
    }
    
}