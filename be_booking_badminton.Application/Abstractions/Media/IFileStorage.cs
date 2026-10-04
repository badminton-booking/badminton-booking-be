using be_booking_badminton.Application.Common.Models;

namespace be_booking_badminton.Application.Abstractions.Media;

public interface IFileStorage
{
    Task<string> UploadAsync(FileUpload file, CancellationToken cancellationToken = default);
}
