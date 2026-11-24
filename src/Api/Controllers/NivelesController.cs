using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NivelesController : ControllerBase
{
    private readonly SaturnoStore _db;
    public NivelesController(SaturnoStore db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Niveles.Query.ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var entity = await _db.Niveles.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Nivel model)
    {
        model.FechaCreacion = DateTime.UtcNow;
        await _db.Niveles.InsertAsync(model);
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Nivel model)
    {
        var existing = await _db.Niveles.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Nombre = model.Nombre;
        existing.Estatus = model.Estatus;
        await _db.Niveles.ReplaceAsync(existing);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Niveles.FindAsync(id);
        if (existing == null) return NotFound();
        if (await _db.Cursos.Query.AnyAsync(c => c.NivelId == id))
            return Conflict(new { error = "El nivel está asociado a uno o más cursos." });
        await _db.Niveles.DeleteAsync(id);
        return NoContent();
    }
}
