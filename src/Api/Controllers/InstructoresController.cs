using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstructoresController : ControllerBase
{
    private readonly NoSqlUContext _db;
    public InstructoresController(NoSqlUContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Instructores.ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var entity = await _db.Instructores.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Instructor model)
    {
        if (string.IsNullOrWhiteSpace(model.Nombre))
            return BadRequest(new { ok = false, error = "El nombre del instructor es obligatorio" });

        var existing = await _db.Instructores.FirstOrDefaultAsync(i => i.Nombre.ToLower() == model.Nombre.ToLower());
        if (existing != null)
        {
            existing.Apellido = model.Apellido ?? existing.Apellido;
            existing.Email = model.Email ?? existing.Email;
            existing.Estatus = "activo";
            await _db.SaveChangesAsync();
            return Ok(new { ok = true, message = "Instructor guardado" });
        }

        model.FechaCreacion = DateTime.UtcNow;
        model.Estatus = model.Estatus ?? "activo";
        _db.Instructores.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Instructor model)
    {
        var existing = await _db.Instructores.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Nombre = model.Nombre;
        existing.Apellido = model.Apellido;
        existing.Email = model.Email;
        existing.Estatus = model.Estatus;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Instructores.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Estatus = "inactivo";
        await _db.SaveChangesAsync();
        return Ok(new { ok = true, message = "Instructor desactivado" });
    }
}
