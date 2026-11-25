using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NoSqlU.Data.Entities;

namespace NoSqlU.Data;

public sealed class SaturnoStore
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Usuario> _empleados;
    private readonly IMongoCollection<Administrador> _administradores;

    public SaturnoStore(IMongoDatabase database)
    {
        _database = database;
        _administradores = database.GetCollection<Administrador>("administradores");
        _empleados = database.GetCollection<Usuario>("empleados");
        Usuarios = new MongoCatalog<Usuario, int>(_empleados, NextIdAsync);
        Categorias = Catalog<Categoria>("categorias");
        Niveles = Catalog<Nivel>("niveles");
        Instructores = Catalog<Instructor>("instructores");
        Cursos = Catalog<Curso>("cursos");
        Estados = Catalog<Estado>("estados");
        Sexos = new MongoCatalog<Sexo, char>(database.GetCollection<Sexo>("sexos"));
    }

    public MongoCatalog<Usuario, int> Usuarios { get; }
    public MongoCatalog<Categoria, int> Categorias { get; }
    public MongoCatalog<Nivel, int> Niveles { get; }
    public MongoCatalog<Instructor, int> Instructores { get; }
    public MongoCatalog<Curso, int> Cursos { get; }
    public MongoCatalog<Estado, int> Estados { get; }
    public MongoCatalog<Sexo, char> Sexos { get; }

    public async Task<Administrador?> BuscarAdministradorAsync(string usuario) =>
        await _administradores.Find(a => a.Usuario == usuario && a.Estatus == "ACTIVO").FirstOrDefaultAsync();

    public Task CrearAdministradorInicialAsync(Administrador administrador) =>
        _administradores.UpdateOneAsync(a => a.Usuario == administrador.Usuario,
            Builders<Administrador>.Update
                .SetOnInsert(a => a.PasswordHash, administrador.PasswordHash)
                .SetOnInsert(a => a.Estatus, administrador.Estatus),
            new UpdateOptions { IsUpsert = true });

    public async Task<bool> EliminarEmpleadoInactivoAsync(int id)
    {
        var result = await _empleados.DeleteOneAsync(u => u.Id == id && u.Estatus == "INACTIVO");
        return result.DeletedCount == 1;
    }

    public async Task<bool> SexoEnUsoAsync(char sexo)
    {
        FilterDefinition<Usuario> filtro = new BsonDocument("id_sexo", sexo.ToString());
        return await _empleados.Find(filtro).AnyAsync();
    }

    public Task<bool> EstadoEnUsoAsync(int id) =>
        Usuarios.Query.AnyAsync(u => u.Direcciones.Any(d => d.EstadoId == id));

    public IQueryable<Usuario> FiltrarEmpleados(string? nombre, int? numero, string? estatus)
    {
        var query = Usuarios.Query;
        if (numero is int id) query = query.Where(u => u.Id == id);
        if (estatus is not null) query = query.Where(u => u.Estatus == estatus);
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            foreach (var palabra in nombre.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                var texto = palabra.ToLowerInvariant();
                query = query.Where(u => u.Nombres.ToLower().Contains(texto) ||
                    (u.ApellidoPaterno != null && u.ApellidoPaterno.ToLower().Contains(texto)) ||
                    (u.ApellidoMaterno != null && u.ApellidoMaterno.ToLower().Contains(texto)));
            }
        }
        return query;
    }

    private MongoCatalog<T, int> Catalog<T>(string nombre) =>
        new(_database.GetCollection<T>(nombre), NextIdAsync);

    private async Task<int> NextIdAsync(string coleccion)
    {
        var counters = _database.GetCollection<BsonDocument>("contadores");
        var counter = await counters.FindOneAndUpdateAsync(
            Builders<BsonDocument>.Filter.Eq("_id", coleccion),
            Builders<BsonDocument>.Update.Inc("valor", 1),
            new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = true, ReturnDocument = ReturnDocument.After });
        return counter["valor"].AsInt32;
    }

    public async Task<bool> CanConnectAsync()
    {
        await _database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
        return await Estados.Query.AnyAsync() && await Sexos.Query.AnyAsync();
    }

    public Task<bool> LoginExistsAsync(string login) =>
        Usuarios.Query.AnyAsync(u => u.Contrasena != null && u.Contrasena.UsuarioLogin == login);

    public Task<bool> TieneCursoAsync(int usuarioId, int cursoId) =>
        Usuarios.Query.AnyAsync(u => u.Id == usuarioId && u.CursosUsuarios.Any(c => c.CursoId == cursoId));

    public async Task<bool> AsignarCursoAsync(int usuarioId, CursoUsuario curso)
    {
        var filtro = Builders<Usuario>.Filter.Where(u => u.Id == usuarioId &&
            !u.CursosUsuarios.Any(c => c.CursoId == curso.CursoId));
        var result = await _empleados.UpdateOneAsync(filtro, Builders<Usuario>.Update.Push(u => u.CursosUsuarios, curso));
        return result.ModifiedCount == 1;
    }

    public Task CambiarEstatusEmpleadoAsync(Usuario usuario) =>
        _empleados.UpdateOneAsync(u => u.Id == usuario.Id, Builders<Usuario>.Update.Set(u => u.Estatus, usuario.Estatus));

    public Task EditarEmpleadoAsync(Usuario usuario) =>
        _empleados.UpdateOneAsync(u => u.Id == usuario.Id, Builders<Usuario>.Update
            .Set(u => u.Nombres, usuario.Nombres)
            .Set(u => u.Estatus, usuario.Estatus)
            .Set(u => u.ApellidoPaterno, usuario.ApellidoPaterno)
            .Set(u => u.ApellidoMaterno, usuario.ApellidoMaterno)
            .Set(u => u.FechaNacimiento, usuario.FechaNacimiento)
            .Set(u => u.FechaRegistro, usuario.FechaRegistro)
            .Set(u => u.SexoId, usuario.SexoId)
            .Set(u => u.CorreoEmpresarial, usuario.CorreoEmpresarial)
            .Set(u => u.Telefono, usuario.Telefono)
            .Set(u => u.Direcciones, usuario.Direcciones)
            .Set(u => u.CursosUsuarios, usuario.CursosUsuarios));

    public async Task<Usuario?> BuscarDetalleEmpleadoAsync(int id)
    {
        var usuario = await Usuarios.FindAsync(id);
        if (usuario is null) return null;

        usuario.Sexo = (await Sexos.FindAsync(usuario.SexoId))!;
        var ids = usuario.Direcciones.Select(d => d.EstadoId).Distinct().ToArray();
        var estados = (await Estados.Query.Where(e => ids.Contains(e.Id)).ToListAsync()).ToDictionary(e => e.Id);
        foreach (var direccion in usuario.Direcciones)
            direccion.Estado = estados.GetValueOrDefault(direccion.EstadoId)!;
        return usuario;
    }

    public async Task<List<Curso>> CargarCursosAsync(int? id = null)
    {
        var cursos = await Cursos.Query.Where(c => !id.HasValue || c.Id == id.GetValueOrDefault()).ToListAsync();
        if (cursos.Count == 0) return cursos;

        var categorias = (await Categorias.Query.ToListAsync()).ToDictionary(c => c.Id);
        var niveles = (await Niveles.Query.ToListAsync()).ToDictionary(n => n.Id);
        var instructores = (await Instructores.Query.ToListAsync()).ToDictionary(i => i.Id);
        foreach (var curso in cursos)
        {
            curso.Categoria = categorias.GetValueOrDefault(curso.CategoriaId);
            curso.Nivel = niveles.GetValueOrDefault(curso.NivelId);
            curso.Instructor = instructores.GetValueOrDefault(curso.InstructorId);
        }
        return cursos;
    }
}
