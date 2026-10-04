using be_booking_badminton.API.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace be_booking_badminton.API.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(ApiResponseFactory.Success(
        new { Status = "Healthy" },
        HttpContext.TraceIdentifier));
}
