using Microsoft.AspNetCore.Mvc;
using NoSqlU.Api.Models;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuariosModel _model;

    public UsuariosController(UsuariosModel model) => _model = model;

    [HttpGet]
    public async Task<IActionResult> GetUsuarios(int? numero, string? nombre, string? estatus, int pagina = 1) =>
        (await _model.GetUsuariosAsync(numero, nombre, estatus, pagina)).ToActionResult(this);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUsuario(int id) =>
        (await _model.GetUsuarioAsync(id)).ToActionResult(this);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id) =>
        (await _model.EliminarAsync(id)).ToActionResult(this);

    [HttpGet("estados")]
    public async Task<IActionResult> GetEstados() =>
        (await _model.GetEstadosAsync()).ToActionResult(this);

    [HttpPost("registro")]
    public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioDto dto) =>
        (await _model.RegistrarAsync(dto)).ToActionResult(this);

    [HttpPut("{id}/estatus")]
    public async Task<IActionResult> CambiarEstatus(int id) =>
        (await _model.CambiarEstatusAsync(id)).ToActionResult(this);

    [HttpGet("cursos-disponibles")]
    public async Task<IActionResult> GetCursosDisponibles() =>
        (await _model.GetCursosDisponiblesAsync()).ToActionResult(this);

    [HttpPost("asignar-curso")]
    public async Task<IActionResult> AsignarCurso([FromBody] AsignarCursoDto dto) =>
        (await _model.AsignarCursoAsync(dto)).ToActionResult(this);
}
