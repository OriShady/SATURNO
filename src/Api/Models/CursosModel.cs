using MongoDB.Driver.Linq;
using NoSqlU.Data;
using NoSqlU.Data.Entities;
using NoSqlU.Api.Models;
using System.Text.Json;

namespace NoSqlU.Api.Models;

public sealed class CursosModel
{
    private readonly SaturnoStore _db;

    private readonly ILogger<CursosModel> _logger;

    public CursosModel(SaturnoStore db, ILogger<CursosModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    internal async Task<ApiResult> GetAllAsync()
    {
        var cursos = (await _db.CargarCursosAsync())
            .Select(c => new
            {
                c.Id,
                c.Nombre,
                c.Descripcion,
                c.Precio,
                c.DuracionMinutos,
                c.FechaPublicacion,
                c.Estatus,
                c.FechaCreacion,
                Categoria = c.Categoria == null ? null : new { c.Categoria.Id, c.Categoria.Nombre },
                Nivel = c.Nivel == null ? null : new { c.Nivel.Id, c.Nivel.Nombre },
                Instructor = c.Instructor == null ? null : new { c.Instructor.Id, c.Instructor.Nombre, c.Instructor.Apellido }
            })
            .ToList();
        return ApiResult.Ok(cursos);
    }

    internal async Task<ApiResult> GetAsync(int id)
    {
        var curso = (await _db.CargarCursosAsync(id)).FirstOrDefault();
        if (curso == null) return ApiResult.NotFound();
        return ApiResult.Ok(CatalogoDto.From(curso));
    }

    internal async Task<ApiResult> CreateAsync(CourseCreateDto dto)
    {
        if (dto == null)
            return ApiResult.BadRequest(new { ok = false, error = "JSON inválido o vacío" });

        var nombre = (dto.Nombre ?? string.Empty).Trim();
        var descripcion = (dto.Descripcion ?? string.Empty).Trim();
        var precio = dto.Precio;
        var duracion = dto.DuracionMinutos;
        var categoriaId = dto.CategoriaId;
        var nivelNombre = (dto.Nivel ?? string.Empty).Trim();
        var instructorNombre = (dto.Instructor ?? string.Empty).Trim();

        if (nombre == string.Empty || descripcion == string.Empty || categoriaId == 0 || (dto.NivelId == null && nivelNombre == string.Empty) || (dto.InstructorId == null && instructorNombre == string.Empty) || dto.FechaPublicacion == default)
            return ApiResult.BadRequest(new { ok = false, error = "Todos los campos son obligatorios" });

        if (precio <= 0) return ApiResult.BadRequest(new { ok = false, error = "El precio debe ser mayor a 0" });
        if (duracion <= 0) return ApiResult.BadRequest(new { ok = false, error = "La duración debe ser mayor a 0" });

        var cat = await _db.Categorias.Query.FirstOrDefaultAsync(c => c.Id == categoriaId && c.Estatus == "activo");
        if (cat == null) return ApiResult.NotFound(new { ok = false, error = "Categoría no existe o está inactiva" });

        try
        {
            int nivelId;
            if (dto.NivelId.HasValue)
            {
                nivelId = dto.NivelId.Value;
                if (await _db.Niveles.FindAsync(nivelId) is null) return ApiResult.BadRequest(new { error = "Nivel inexistente." });
            }
            else
            {
                var existingNivel = await _db.Niveles.Query.FirstOrDefaultAsync(n => n.Nombre.ToLower() == nivelNombre.ToLower());
                if (existingNivel != null)
                {
                    existingNivel.Estatus = "activo";
                    await _db.Niveles.ReplaceAsync(existingNivel);
                    nivelId = existingNivel.Id;
                }
                else
                {
                    var n = new Nivel { Nombre = nivelNombre, Estatus = "activo", FechaCreacion = DateTime.UtcNow };
                    await _db.Niveles.InsertAsync(n);
                    nivelId = n.Id;
                }
            }

            int instructorId;
            if (dto.InstructorId.HasValue)
            {
                instructorId = dto.InstructorId.Value;
                if (await _db.Instructores.FindAsync(instructorId) is null) return ApiResult.BadRequest(new { error = "Instructor inexistente." });
            }
            else
            {
                var existingInst = await _db.Instructores.Query.FirstOrDefaultAsync(i => i.Nombre.ToLower() == instructorNombre.ToLower());
                if (existingInst != null)
                {
                    existingInst.Estatus = "activo";
                    await _db.Instructores.ReplaceAsync(existingInst);
                    instructorId = existingInst.Id;
                }
                else
                {
                    var ins = new Instructor { Nombre = instructorNombre, Apellido = string.Empty, Email = string.Empty, Estatus = "activo", FechaCreacion = DateTime.UtcNow };
                    await _db.Instructores.InsertAsync(ins);
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

            await _db.Cursos.InsertAsync(curso);

            return ApiResult.Created(curso.Id, new { ok = true, inserted_id = curso.Id.ToString() });
        }
        catch (MongoDB.Driver.MongoException e)
        {
            _logger.LogError(e, "Error al guardar el curso.");
            return ApiResult.BadRequest(new { ok = false, error = "No se pudo crear el curso" });
        }
    }

    internal async Task<ApiResult> UpdateAsync(int id, JsonElement body)
    {
        if (!body.TryGetProperty("estatus", out var est))
            return ApiResult.BadRequest(new { ok = false, error = "No hay datos para actualizar" });

        var estatus = est.GetString() == "inactivo" ? "inactivo" : "activo";
        var existing = await _db.Cursos.FindAsync(id);
        if (existing == null) return ApiResult.NotFound();
        existing.Estatus = estatus;
        await _db.Cursos.ReplaceAsync(existing);
        return ApiResult.Ok(new { ok = true, message = "Curso actualizado" });
    }

    internal async Task<ApiResult> DeleteAsync(int id)
    {
        var existing = await _db.Cursos.FindAsync(id);
        if (existing == null) return ApiResult.NotFound();
        existing.Estatus = "inactivo";
        await _db.Cursos.ReplaceAsync(existing);
        return ApiResult.Ok(new { ok = true, message = "Curso desactivado" });
    }
}
