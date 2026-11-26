using Microsoft.AspNetCore.Mvc;
using NoSqlU.Api.Models;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FiltroController : ControllerBase
{
    private readonly FiltroModel _model;

    public FiltroController(FiltroModel model) => _model = model;

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? q,
        [FromQuery] string? categoria,
        [FromQuery] string? nivel,
        [FromQuery] string? instructor,
        [FromQuery] string? estatus,
        [FromQuery] decimal? min,
        [FromQuery] decimal? max) =>
        (await _model.BuscarAsync(q, categoria, nivel, instructor, estatus, min, max)).ToActionResult(this);
}
