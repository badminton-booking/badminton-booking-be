namespace be_booking_badminton.Application.Services.Sample;

public interface IService
{
    Task<Response.EchoResponse> EchoAsync(
        Request.EchoRequest request,
        CancellationToken cancellationToken = default);
}
