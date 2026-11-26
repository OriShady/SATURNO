using Microsoft.AspNetCore.Mvc;
using NoSqlU.Api.Models;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.AllowAnonymous]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthModel _model;

    public HealthController(HealthModel model) => _model = model;

    [HttpGet]
    public async Task<IActionResult> Get() =>
        (await _model.GetAsync()).ToActionResult(this);
}
