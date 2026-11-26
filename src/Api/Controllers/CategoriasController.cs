using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NoSqlU.Data;
using NoSqlU.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly SaturnoStore _db;

    public CategoriasController(SaturnoStore db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _db.Categorias.Query
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return Ok(categorias);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        if (id <= 0)
            return BadRequest(new { ok = false, error = "El identificador debe ser mayor a 0" });

        var entity = await _db.Categorias.FindAsync(id);
        if (entity == null)
            return NotFound(new { ok = false, error = "Categoría no encontrada" });

        return Ok(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoriaCreateDto dto)
    {
        if (dto == null)
            return BadRequest(new { ok = false, error = "El cuerpo de la solicitud es obligatorio" });

        var nombre = dto.Nombre?.Trim();
        var descripcion = dto.Descripcion?.Trim();

        var error = ValidarNombre(nombre);
        if (error != null)
            return BadRequest(new { ok = false, error });

        if (descripcion?.Length > 255)
            return BadRequest(new { ok = false, error = "La descripción no puede exceder 255 caracteres" });

        var estatus = string.IsNullOrWhiteSpace(dto.Estatus)
            ? "activo"
            : dto.Estatus.Trim().ToLowerInvariant();

        if (!EsEstatusValido(estatus))
            return BadRequest(new { ok = false, error = "El estatus debe ser activo o inactivo" });

        var nombreDuplicado = await _db.Categorias.Query
            .AnyAsync(c => c.Nombre.ToLower() == nombre!.ToLower());

        if (nombreDuplicado)
            return Conflict(new { ok = false, error = "Ya existe una categoría con ese nombre" });

        var categoria = new Categoria
        {
            Nombre = nombre!,
            Descripcion = descripcion,
            Estatus = estatus,
            FechaCreacion = DateTime.UtcNow
        };

        try
        {
            await _db.Categorias.InsertAsync(categoria);
            return CreatedAtAction(nameof(Get), new { id = categoria.Id }, categoria);
        }
        catch (MongoWriteException)
        {
            return Conflict(new { ok = false, error = "No se pudo guardar la categoría" });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoriaUpdateDto dto)
    {
        if (id <= 0)
            return BadRequest(new { ok = false, error = "El identificador debe ser mayor a 0" });

        if (dto == null)
            return BadRequest(new { ok = false, error = "El cuerpo de la solicitud es obligatorio" });

        var existing = await _db.Categorias.FindAsync(id);
        if (existing == null)
            return NotFound(new { ok = false, error = "Categoría no encontrada" });

        if (dto.Nombre != null)
        {
            var nombre = dto.Nombre.Trim();
            var error = ValidarNombre(nombre);
            if (error != null)
                return BadRequest(new { ok = false, error });

            var nombreDuplicado = await _db.Categorias.Query
                .AnyAsync(c => c.Id != id && c.Nombre.ToLower() == nombre.ToLower());

            if (nombreDuplicado)
                return Conflict(new { ok = false, error = "Ya existe una categoría con ese nombre" });

            existing.Nombre = nombre;
        }

        if (dto.Descripcion != null)
        {
            var descripcion = dto.Descripcion.Trim();
            if (descripcion.Length > 255)
                return BadRequest(new { ok = false, error = "La descripción no puede exceder 255 caracteres" });

            existing.Descripcion = descripcion;
        }

        if (!string.IsNullOrWhiteSpace(dto.Estatus))
        {
            var estatus = dto.Estatus.Trim().ToLowerInvariant();
            if (!EsEstatusValido(estatus))
                return BadRequest(new { ok = false, error = "El estatus debe ser activo o inactivo" });

            existing.Estatus = estatus;
        }

        try
        {
            await _db.Categorias.ReplaceAsync(existing);
            return Ok(new { ok = true, message = "Categoría actualizada", categoria = existing });
        }
        catch (MongoWriteException)
        {
            return Conflict(new { ok = false, error = "No se pudo actualizar la categoría" });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest(new { ok = false, error = "El identificador debe ser mayor a 0" });

        var existing = await _db.Categorias.FindAsync(id);
        if (existing == null)
            return NotFound(new { ok = false, error = "Categoría no encontrada" });

        if (existing.Estatus == "inactivo")
            return Ok(new { ok = true, message = "La categoría ya estaba inactiva" });

        existing.Estatus = "inactivo";

        try
        {
            await _db.Categorias.ReplaceAsync(existing);
            return Ok(new { ok = true, message = "Categoría desactivada" });
        }
        catch (MongoWriteException)
        {
            return Conflict(new { ok = false, error = "No se pudo desactivar la categoría" });
        }
    }

    private static string? ValidarNombre(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return "El nombre es obligatorio";

        if (nombre.Length > 100)
            return "El nombre no puede exceder 100 caracteres";

        return null;
    }

    private static bool EsEstatusValido(string estatus) =>
        estatus == "activo" || estatus == "inactivo";
}

public class CategoriaCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string? Nombre { get; set; }

    [StringLength(255)]
    public string? Descripcion { get; set; }

    public string? Estatus { get; set; }
}

public class CategoriaUpdateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Nombre { get; set; }

    [StringLength(255)]
    public string? Descripcion { get; set; }

    public string? Estatus { get; set; }
}
