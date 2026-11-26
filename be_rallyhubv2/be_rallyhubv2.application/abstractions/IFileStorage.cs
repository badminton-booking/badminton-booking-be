namespace be_rallyhubv2.application.abstractions;

public sealed record FileUpload(Stream Content, string FileName, string ContentType, string Folder);

public sealed record StoredFile(string Url, string StorageKey);

public interface IFileStorage
{
    Task<StoredFile> UploadAsync(FileUpload upload, CancellationToken cancellationToken);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}