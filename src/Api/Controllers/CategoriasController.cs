using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly NoSqlUContext _db;
    public CategoriasController(NoSqlUContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Categorias.ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var entity = await _db.Categorias.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Categoria model)
    {
        model.FechaCreacion = DateTime.UtcNow;
        _db.Categorias.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Categoria model)
    {
        var existing = await _db.Categorias.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Nombre = model.Nombre;
        existing.Descripcion = model.Descripcion;
        existing.Estatus = model.Estatus;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Categorias.FindAsync(id);
        if (existing == null) return NotFound();
        //_db.Categorias.Remove(existing);
        existing.Estatus = "inactivo";
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
