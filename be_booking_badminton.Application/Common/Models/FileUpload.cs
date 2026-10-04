namespace be_booking_badminton.Application.Common.Models;

public sealed record FileUpload(
    string FileName,
    string ContentType,
    long Length,
    Stream Content);
