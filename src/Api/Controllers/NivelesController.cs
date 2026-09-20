using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NivelesController : ControllerBase
{
    private readonly NoSqlUContext _db;
    public NivelesController(NoSqlUContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Niveles.ToListAsync());

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
        _db.Niveles.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Nivel model)
    {
        var existing = await _db.Niveles.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Nombre = model.Nombre;
        existing.Estatus = model.Estatus;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Niveles.FindAsync(id);
        if (existing == null) return NotFound();
        _db.Niveles.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
