using MongoDB.Driver.Linq;
using NoSqlU.Data;
using NoSqlU.Domain.Entities;

namespace NoSqlU.Api.Models.Catalogos;

public sealed class CatalogosService
{
    private const int PageSize = 5;
    private readonly SaturnoStore _db;

    public CatalogosService(SaturnoStore db) => _db = db;

    public async Task<(bool Encontrado, string? ErrorClave)> GuardarAsync(
        string tipo, CatalogoElementoViewModel elemento, string? claveOriginal)
    {
        var editar = !string.IsNullOrWhiteSpace(claveOriginal);
        switch (tipo)
        {
            case "sexos":
                var claveSexo = elemento.Clave!.Trim().ToUpperInvariant()[0];
                var sexo = editar
                    ? await _db.Sexos.FindAsync(char.Parse(claveOriginal!))
                    : new Sexo { Id = claveSexo };
                if (sexo is null) return (false, null);
                if (editar && sexo.Id != claveSexo)
                    return (true, "No es posible cambiar la clave de un sexo existente.");
                sexo.Descripcion = elemento.Nombre.Trim();

                if (editar) await _db.Sexos.ReplaceAsync(sexo);
                else await _db.Sexos.InsertAsync(sexo);
                break;
            case "estados":
                var estado = editar
                    ? await _db.Estados.Query.FirstOrDefaultAsync(e => e.Id == int.Parse(claveOriginal!))
                    : new Estado();
                if (estado is null) return (false, null);
                estado.NombreEstado = elemento.Nombre.Trim();

                if (editar) await _db.Estados.ReplaceAsync(estado);
                else await _db.Estados.InsertAsync(estado);
                break;
        }
        return (true, null);
    }

    public async Task<(bool Encontrado, string? Conflicto)> CambiarEstatusAsync(string tipo, string clave)
    {
        switch (tipo)
        {
            case "sexos":
                var sexo = await _db.Sexos.FindAsync(char.Parse(clave));
                if (sexo is null) return (false, null);
                if (sexo.Estatus == "ACTIVO" && await _db.SexoEnUsoAsync(sexo.Id))
                    return (true, "Este sexo está en uso.");
                sexo.Estatus = sexo.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
                await _db.Sexos.ReplaceAsync(sexo);
                break;
            case "estados":
                var estado = await _db.Estados.FindAsync(int.Parse(clave));
                if (estado is null) return (false, null);
                if (estado.Estatus == "ACTIVO" && await _db.EstadoEnUsoAsync(estado.Id))
                    return (true, "Este estado está en uso.");
                estado.Estatus = estado.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
                await _db.Estados.ReplaceAsync(estado);
                break;
        }
        return (true, null);
    }

    public async Task<CatalogosViewModel> PrepararVistaAsync(
        string tipo,
        string? editar = null,
        CatalogoElementoViewModel? elemento = null,
        bool mostrarFormulario = false,
        int pagina = 1)
    {
        var model = new CatalogosViewModel
        {
            Tipo = tipo,
            ClaveOriginal = editar,
            EsEdicion = !string.IsNullOrWhiteSpace(editar),
            MostrarFormulario = mostrarFormulario
        };
        switch (tipo)
        {
            case "sexos":
                model.TotalElementos = await _db.Sexos.Query.CountAsync();
                break;
            case "estados":
                model.TotalElementos = await _db.Estados.Query.CountAsync();
                break;
        }
        model.TotalPaginas = Math.Max(1, (int)Math.Ceiling(model.TotalElementos / (double)PageSize));
        model.Pagina = Math.Clamp(pagina, 1, model.TotalPaginas);
        switch (tipo)
        {
            case "sexos":
                var sexos = await _db.Sexos.Query.OrderBy(s => s.Descripcion)
                    .Skip((model.Pagina - 1) * PageSize).Take(PageSize).ToListAsync();
                model.Elementos = sexos.Select(s => new CatalogoFilaViewModel
                    { Clave = s.Id.ToString(), Nombre = s.Descripcion, Estatus = s.Estatus }).ToList();
                if (elemento is null && !string.IsNullOrWhiteSpace(editar) && char.TryParse(editar, out var claveSexo))
                {
                    var item = await _db.Sexos.FindAsync(claveSexo);
                    if (item is null) return model;
                    elemento = new CatalogoElementoViewModel { Clave = item.Id.ToString(), Nombre = item.Descripcion, Estatus = item.Estatus.ToLowerInvariant() };
                }
                break;
            case "estados":
                model.Elementos = await _db.Estados.Query.OrderBy(e => e.NombreEstado)
                    .Skip((model.Pagina - 1) * PageSize).Take(PageSize)
                    .Select(e => new CatalogoFilaViewModel { Clave = e.Id.ToString(), Nombre = e.NombreEstado, Estatus = e.Estatus }).ToListAsync();
                if (elemento is null && int.TryParse(editar, out var idEstado))
                {
                    var item = await _db.Estados.Query.FirstOrDefaultAsync(e => e.Id == idEstado);
                    if (item is null) return model;
                    elemento = new CatalogoElementoViewModel { Nombre = item.NombreEstado, Estatus = item.Estatus.ToLowerInvariant() };
                }
                break;
        }
        await Task.WhenAll(model.Elementos.Select(async item => item.EnUso = tipo == "sexos"
            ? await _db.SexoEnUsoAsync(char.Parse(item.Clave))
            : await _db.EstadoEnUsoAsync(int.Parse(item.Clave))));
        model.Elemento = elemento ?? new CatalogoElementoViewModel();
        return model;
    }

    public async Task<bool> ExisteDuplicadoAsync(string tipo, string nombre, string? clave, string? claveOriginal)
    {
        var originalId = int.TryParse(claveOriginal, out var parsedId) ? parsedId : (int?)null;
        var originalSex = char.TryParse(claveOriginal, out var parsedSex) ? parsedSex : (char?)null;
        switch (tipo)
        {
            case "sexos":
                if (string.IsNullOrWhiteSpace(clave)) return false;
                var codigo = clave.Trim().ToUpperInvariant()[0];
                var sexos = await _db.Sexos.Query.ToListAsync();
                return sexos.Any(s => s.Id != originalSex &&
                    (s.Id == codigo || string.Equals(s.Descripcion, nombre, StringComparison.OrdinalIgnoreCase)));
            case "estados":
                return await _db.Estados.Query.AnyAsync(e => e.NombreEstado == nombre && (!originalId.HasValue || e.Id != originalId.Value));
            default:
                return false;
        }
    }
}
