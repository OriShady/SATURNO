using Microsoft.AspNetCore.Mvc;
using NoSqlU.Data;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly NoSqlUContext _db;
    public HealthController(NoSqlUContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var canConnect = await _db.Database.CanConnectAsync();
            if (canConnect) return Ok(new { db = "ok" });
            return StatusCode(503, new { db = "unavailable" });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { db = "error", detail = e.Message });
        }
    }
}
