using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoSqlU.Data;
using NoSqlU.Data.Entities;
using NoSqlU.Api.Models;
using System.Text.Json;

namespace NoSqlU.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CursosController : ControllerBase
{
    private readonly NoSqlUContext _db;

    public CursosController(NoSqlUContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cursos = await _db.Cursos.Include(c => c.Categoria).Include(c => c.Nivel).Include(c => c.Instructor).ToListAsync();
        return Ok(cursos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var curso = await _db.Cursos.Include(c => c.Categoria).Include(c => c.Nivel).Include(c => c.Instructor).FirstOrDefaultAsync(c => c.Id == id);
        if (curso == null) return NotFound();
        return Ok(curso);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
    {
        if (dto == null)
            return BadRequest(new { ok = false, error = "JSON inválido o vacío" });

        var nombre = (dto.Nombre ?? string.Empty).Trim();
        var descripcion = (dto.Descripcion ?? string.Empty).Trim();
        var precio = dto.Precio;
        var duracion = dto.DuracionMinutos;
        var categoriaId = dto.CategoriaId;
        var nivelNombre = (dto.Nivel ?? string.Empty).Trim();
        var instructorNombre = (dto.Instructor ?? string.Empty).Trim();

        if (nombre == string.Empty || descripcion == string.Empty || categoriaId == 0 || (dto.NivelId == null && nivelNombre == string.Empty) || (dto.InstructorId == null && instructorNombre == string.Empty) || dto.FechaPublicacion == default)
            return BadRequest(new { ok = false, error = "Todos los campos son obligatorios" });

        if (precio <= 0) return BadRequest(new { ok = false, error = "El precio debe ser mayor a 0" });
        if (duracion == 0) return BadRequest(new { ok = false, error = "La duración debe ser mayor a 0" });

        var cat = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == categoriaId && c.Estatus == "activo");
        if (cat == null) return NotFound(new { ok = false, error = "Categoría no existe o está inactiva" });

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            int nivelId;
            if (dto.NivelId.HasValue)
            {
                nivelId = dto.NivelId.Value;
            }
            else
            {
                var existingNivel = await _db.Niveles.FirstOrDefaultAsync(n => n.Nombre.ToLower() == nivelNombre.ToLower());
                if (existingNivel != null)
                {
                    existingNivel.Estatus = "activo";
                    nivelId = existingNivel.Id;
                }
                else
                {
                    var n = new Nivel { Nombre = nivelNombre, Estatus = "activo", FechaCreacion = DateTime.UtcNow };
                    _db.Niveles.Add(n);
                    await _db.SaveChangesAsync();
                    nivelId = n.Id;
                }
            }

            int instructorId;
            if (dto.InstructorId.HasValue)
            {
                instructorId = dto.InstructorId.Value;
            }
            else
            {
                var existingInst = await _db.Instructores.FirstOrDefaultAsync(i => i.Nombre.ToLower() == instructorNombre.ToLower());
                if (existingInst != null)
                {
                    existingInst.Estatus = "activo";
                    instructorId = existingInst.Id;
                }
                else
                {
                    var ins = new Instructor { Nombre = instructorNombre, Apellido = string.Empty, Email = string.Empty, Estatus = "activo", FechaCreacion = DateTime.UtcNow };
                    _db.Instructores.Add(ins);
                    await _db.SaveChangesAsync();
                    instructorId = ins.Id;
                }
            }

            var curso = new Curso
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Precio = precio,
                DuracionMinutos = duracion,
                FechaPublicacion = dto.FechaPublicacion,
                CategoriaId = categoriaId,
                NivelId = nivelId,
                InstructorId = instructorId,
                Estatus = "activo",
                FechaCreacion = DateTime.UtcNow
            };

            _db.Cursos.Add(curso);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return CreatedAtAction(nameof(Get), new { id = curso.Id }, new { ok = true, inserted_id = curso.Id.ToString() });
        }
        catch (Exception e)
        {
            await tx.RollbackAsync();
            return BadRequest(new { ok = false, error = "No se pudo crear el curso", detalle = e.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] JsonElement body)
    {
        if (!body.TryGetProperty("estatus", out var est))
            return BadRequest(new { ok = false, error = "No hay datos para actualizar" });

        var estatus = est.GetString() == "inactivo" ? "inactivo" : "activo";
        var existing = await _db.Cursos.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Estatus = estatus;
        await _db.SaveChangesAsync();
        return Ok(new { ok = true, message = "Curso actualizado" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Cursos.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Estatus = "inactivo";
        await _db.SaveChangesAsync();
        return Ok(new { ok = true, message = "Curso desactivado" });
    }
}
