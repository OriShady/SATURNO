using MongoDB.Driver.Linq;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Models;

public sealed class NivelesModel
{
    private readonly SaturnoStore _db;
    public NivelesModel(SaturnoStore db) => _db = db;

    internal async Task<ApiResult> GetAllAsync() => ApiResult.Ok((await _db.Niveles.Query.ToListAsync()).Select(CatalogoDto.From).ToList());

    internal async Task<ApiResult> GetAsync(int id)
    {
        var entity = await _db.Niveles.FindAsync(id);
        if (entity == null) return ApiResult.NotFound();
        return ApiResult.Ok(CatalogoDto.From(entity));
    }

    internal async Task<ApiResult> CreateAsync(NivelInput input)
    {
        var model = new Nivel { Nombre = input.Nombre, Estatus = input.Estatus };
        model.FechaCreacion = DateTime.UtcNow;
        await _db.Niveles.InsertAsync(model);
        return ApiResult.Created(model.Id, CatalogoDto.From(model));
    }

    internal async Task<ApiResult> UpdateAsync(int id, NivelInput input)
    {
        var existing = await _db.Niveles.FindAsync(id);
        if (existing == null) return ApiResult.NotFound();
        existing.Nombre = input.Nombre;
        existing.Estatus = input.Estatus;
        await _db.Niveles.ReplaceAsync(existing);
        return ApiResult.NoContent();
    }

    internal async Task<ApiResult> DeleteAsync(int id)
    {
        var existing = await _db.Niveles.FindAsync(id);
        if (existing == null) return ApiResult.NotFound();
        if (await _db.Cursos.Query.AnyAsync(c => c.NivelId == id))
            return ApiResult.Conflict(new { error = "El nivel está asociado a uno o más cursos." });
        await _db.Niveles.DeleteAsync(id);
        return ApiResult.NoContent();
    }
}
