using be_booking_badminton.API.Contracts;
using Microsoft.AspNetCore.Mvc;
using SampleService = be_booking_badminton.Application.Services.Sample;

namespace be_booking_badminton.API.Controllers;

[ApiController]
[Route("api/sample")]
public sealed class SampleController : ControllerBase
{
    private readonly SampleService.IService _service;

    public SampleController(SampleService.IService service)
    {
        _service = service;
    }

    [HttpPost("echo")]
    public async Task<IActionResult> Echo(
        [FromBody] SampleService.Request.EchoRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _service.EchoAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, HttpContext.TraceIdentifier));
    }
}
