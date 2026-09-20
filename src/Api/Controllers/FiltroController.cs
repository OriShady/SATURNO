using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoSqlU.Data;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FiltroController : ControllerBase
{
    private readonly NoSqlUContext _db;
    public FiltroController(NoSqlUContext db) => _db = db;

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? q,
        [FromQuery] string? categoria,
        [FromQuery] string? nivel,
        [FromQuery] string? instructor,
        [FromQuery] string? estatus,
        [FromQuery] decimal? min,
        [FromQuery] decimal? max)
    {
        var query = _db.Cursos
            .Include(c => c.Categoria)
            .Include(c => c.Nivel)
            .Include(c => c.Instructor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c =>
                c.Nombre.Contains(q) ||
                c.Descripcion.Contains(q));

        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(c => c.Categoria.Nombre == categoria);

        if (!string.IsNullOrWhiteSpace(nivel))
            query = query.Where(c => c.Nivel.Nombre == nivel);

        if (!string.IsNullOrWhiteSpace(instructor))
            query = query.Where(c => c.Instructor.Nombre == instructor);

        if (!string.IsNullOrWhiteSpace(estatus))
            query = query.Where(c => c.Estatus == estatus);

        if (min.HasValue)
            query = query.Where(c => c.Precio >= min.Value);

        if (max.HasValue)
            query = query.Where(c => c.Precio <= max.Value);

        return Ok(await query.ToListAsync());
    }
}
