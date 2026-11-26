using NoSqlU.Data;

namespace NoSqlU.Api.Models;

public sealed class FiltroModel
{
    private readonly SaturnoStore _db;
    public FiltroModel(SaturnoStore db) => _db = db;

    internal async Task<ApiResult> BuscarAsync(
        string? q,
        string? categoria,
        string? nivel,
        string? instructor,
        string? estatus,
        decimal? min,
        decimal? max)
    {
        var query = (await _db.CargarCursosAsync()).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c =>
                c.Nombre.Contains(q) ||
                c.Descripcion.Contains(q));

        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(c => c.Categoria?.Nombre == categoria);

        if (!string.IsNullOrWhiteSpace(nivel))
            query = query.Where(c => c.Nivel?.Nombre == nivel);

        if (!string.IsNullOrWhiteSpace(instructor))
            query = query.Where(c => c.Instructor?.Nombre == instructor);

        if (!string.IsNullOrWhiteSpace(estatus))
            query = query.Where(c => c.Estatus == estatus);

        if (min.HasValue)
            query = query.Where(c => c.Precio >= min.Value);

        if (max.HasValue)
            query = query.Where(c => c.Precio <= max.Value);

        return ApiResult.Ok(query.Select(CatalogoDto.From).ToList());
    }
}
