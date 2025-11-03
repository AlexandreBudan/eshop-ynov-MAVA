
using Microsoft.AspNetCore.Mvc;

namespace Discount.Grpc.Controllers;

/// <summary>
/// Provides an endpoint to check the health of the service.
/// </summary>
[ApiController]
[Route("api/health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns a 200 OK status to indicate the service is running and healthy.
    /// </summary>
    /// <returns>An OK result with a status message.</returns>
    /// <response code="200">Service is healthy</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new { Status = "Healthy" });
    }
}
