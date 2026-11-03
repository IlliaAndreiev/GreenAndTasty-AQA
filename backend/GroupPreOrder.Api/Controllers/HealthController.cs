using Microsoft.AspNetCore.Mvc;

namespace GroupPreOrder.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            message = "GroupPreOrder API is running"
        });
    }
}