using MongoDB.Driver.Linq;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Models;

public sealed class InstructoresModel
{
    private readonly SaturnoStore _db;
    public InstructoresModel(SaturnoStore db) => _db = db;

    internal async Task<ApiResult> GetAllAsync() => ApiResult.Ok((await _db.Instructores.Query.ToListAsync()).Select(CatalogoDto.From).ToList());

    internal async Task<ApiResult> GetAsync(int id)
    {
        var entity = await _db.Instructores.FindAsync(id);
        if (entity == null) return ApiResult.NotFound();
        return ApiResult.Ok(CatalogoDto.From(entity));
    }

    internal async Task<ApiResult> CreateAsync(InstructorInput input)
    {
        var model = new Instructor { Nombre = input.Nombre, Apellido = input.Apellido, Email = input.Email, Estatus = input.Estatus };
        if (string.IsNullOrWhiteSpace(model.Nombre))
            return ApiResult.BadRequest(new { ok = false, error = "El nombre del instructor es obligatorio" });

        var existing = await _db.Instructores.Query.FirstOrDefaultAsync(i => i.Nombre.ToLower() == model.Nombre.ToLower());
        if (existing != null)
        {
            existing.Apellido = model.Apellido ?? existing.Apellido;
            existing.Email = model.Email ?? existing.Email;
            existing.Estatus = "activo";
            await _db.Instructores.ReplaceAsync(existing);
            return ApiResult.Ok(new { ok = true, message = "Instructor guardado" });
        }

        model.FechaCreacion = DateTime.UtcNow;
        model.Estatus = model.Estatus ?? "activo";
        await _db.Instructores.InsertAsync(model);
        return ApiResult.Created(model.Id, CatalogoDto.From(model));
    }

    internal async Task<ApiResult> UpdateAsync(int id, InstructorInput input)
    {
        var existing = await _db.Instructores.FindAsync(id);
        if (existing == null) return ApiResult.NotFound();
        existing.Nombre = input.Nombre;
        existing.Apellido = input.Apellido;
        existing.Email = input.Email;
        existing.Estatus = input.Estatus;
        await _db.Instructores.ReplaceAsync(existing);
        return ApiResult.NoContent();
    }

    internal async Task<ApiResult> DeleteAsync(int id)
    {
        var existing = await _db.Instructores.FindAsync(id);
        if (existing == null) return ApiResult.NotFound();
        existing.Estatus = "inactivo";
        await _db.Instructores.ReplaceAsync(existing);
        return ApiResult.Ok(new { ok = true, message = "Instructor desactivado" });
    }
}
