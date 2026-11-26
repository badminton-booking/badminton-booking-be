using be_rallyhubv2.application.abstractions;
using be_rallyhubv2.infrastructure.files;
using Microsoft.Extensions.Options;

namespace be_rallyhubv2.tests.unit.infrastructure;

public class CloudinaryFileStorageTests
{
    private static CloudinaryFileStorage Create(long maxBytes = 1000)
        => new(Options.Create(new CloudinaryOptions
        {
            CloudName = "demo",
            ApiKey = "key",
            ApiSecret = "secret",
            MaxFileSizeBytes = maxBytes
        }));

    private static FileUpload Upload(string fileName, string contentType, int size)
        => new(new MemoryStream(new byte[size]), fileName, contentType, "facilities");

    [Fact]
    public async Task Non_image_extension_is_rejected_before_any_network_call()
        => await Assert.ThrowsAsync<ArgumentException>(
            () => Create().UploadAsync(Upload("virus.exe", "image/png", 10), default));

    [Fact]
    public async Task Non_image_content_type_is_rejected()
        => await Assert.ThrowsAsync<ArgumentException>(
            () => Create().UploadAsync(Upload("photo.png", "application/pdf", 10), default));

    [Fact]
    public async Task Empty_file_is_rejected()
        => await Assert.ThrowsAsync<ArgumentException>(
            () => Create().UploadAsync(Upload("photo.png", "image/png", 0), default));

    [Fact]
    public async Task File_larger_than_the_limit_is_rejected()
        => await Assert.ThrowsAsync<ArgumentException>(
            () => Create(maxBytes: 100).UploadAsync(Upload("photo.png", "image/png", 101), default));
}