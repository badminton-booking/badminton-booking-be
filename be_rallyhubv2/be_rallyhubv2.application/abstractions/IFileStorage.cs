namespace be_rallyhubv2.application.abstractions;

public sealed record FileUpload(Stream Content, string Filename
    , string ContentType, string Folder);

public sealed record StoredFile(string Url, string StorageKey);

public interface IFileStorage
{
    Task<StoredFile> UpLoadAsync(FileUpload upload, CancellationToken cancellationToken);
    Task DeleteAsync(string storageKey,  CancellationToken cancellationToken);
}