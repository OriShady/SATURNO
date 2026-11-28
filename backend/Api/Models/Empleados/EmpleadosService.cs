using MongoDB.Driver.Linq;
using NoSqlU.Data;
using NoSqlU.Domain.Entities;

namespace NoSqlU.Api.Models.Empleados;

public sealed class EmpleadosService
{
    private const int PageSize = 5;
    private readonly SaturnoStore _db;

    public EmpleadosService(SaturnoStore db) => _db = db;

    public async Task<bool> ExisteAsync(int id) => await _db.Usuarios.FindAsync(id) is not null;

    public async Task<EmpleadoDetalleViewModel?> BuscarDetalleAsync(int id)
    {
        var usuario = await _db.BuscarDetalleEmpleadoAsync(id);
        if (usuario is null) return null;

        return new EmpleadoDetalleViewModel
        {
            Id = usuario.Id,
            Nombres = usuario.Nombres,
            ApellidoPaterno = usuario.ApellidoPaterno,
            ApellidoMaterno = usuario.ApellidoMaterno,
            CorreoEmpresarial = usuario.CorreoEmpresarial,
            Estatus = usuario.Estatus,
            FechaRegistro = usuario.FechaRegistro,
            FechaNacimiento = usuario.FechaNacimiento,
            Sexo = usuario.Sexo?.Descripcion,
            Telefono = usuario.Telefono,
            Direcciones = usuario.Direcciones.Select(d => new DireccionEmpleadoViewModel
            {
                Calle = d.Calle,
                NumeroExterior = d.NumeroExterior,
                NumeroInterior = d.NumeroInterior,
                Colonia = d.Colonia,
                CodigoPostal = d.CodigoPostal,
                Municipio = d.Municipio,
                Estado = d.Estado?.NombreEstado
            }).ToList(),
            CursosUsuarios = usuario.CursosUsuarios.Select(c => new CursoEmpleadoViewModel
            {
                NombreCurso = c.NombreCurso,
                DuracionMinutos = c.DuracionMinutos,
                FechaInicio = c.FechaInicio,
                FechaEntregaConstancia = c.FechaEntregaConstancia ?? c.FechaFinalizacion
            }).ToList()
        };
    }

    public Task<bool> EliminarAsync(int id) => _db.EliminarEmpleadoInactivoAsync(id);

    public async Task<int> RegistrarAsync(EmpleadoFormViewModel model)
    {
        var usuario = new Usuario { Estatus = "ACTIVO" };
        AplicarFormulario(usuario, model);
        await _db.Usuarios.InsertAsync(usuario);
        return usuario.Id;
    }

    public async Task<bool> EditarAsync(EmpleadoFormViewModel model)
    {
        var usuario = await _db.Usuarios.FindAsync(model.Id);
        if (usuario is null) return false;
        AplicarFormulario(usuario, model);
        usuario.Estatus = model.Estatus;
        await _db.EditarEmpleadoAsync(usuario);
        return true;
    }

    public async Task<bool> DarDeBajaAsync(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return false;

        usuario.Estatus = "INACTIVO";
        await _db.CambiarEstatusEmpleadoAsync(usuario);
        return true;
    }

    public async Task<EmpleadoFormViewModel?> PrepararEdicionAsync(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return null;

        var direccion = usuario.Direcciones.FirstOrDefault();
        var model = new EmpleadoFormViewModel
        {
            Id = usuario.Id,
            Estatus = usuario.Estatus,
            Nombres = usuario.Nombres,
            ApellidoPaterno = usuario.ApellidoPaterno ?? string.Empty,
            ApellidoMaterno = usuario.ApellidoMaterno,
            FechaNacimiento = usuario.FechaNacimiento,
            FechaIngreso = usuario.FechaRegistro,
            SexoId = usuario.SexoId,
            CorreoEmpresarial = usuario.CorreoEmpresarial,
            Telefono = usuario.Telefono,
            EstadoId = direccion?.EstadoId ?? 0,
            Calle = direccion?.Calle ?? string.Empty,
            NumeroExterior = direccion?.NumeroExterior ?? string.Empty,
            NumeroInterior = direccion?.NumeroInterior,
            CodigoPostal = direccion?.CodigoPostal ?? string.Empty,
            Cursos = usuario.CursosUsuarios.Select((c, indice) => new CursoEmpleadoFormViewModel
            {
                NombreCurso = c.NombreCurso,
                DuracionMinutos = c.DuracionMinutos,
                FechaInicio = c.FechaInicio,
                FechaEntregaConstancia = c.FechaEntregaConstancia ?? c.FechaFinalizacion,
                IndiceOriginal = indice
            }).ToList()
        };
        return await PrepararFormularioAsync(model);
    }

    public async Task<EmpleadosViewModel> ListarAsync(string? q, string? estatus, int pagina, int? numero)
    {
        var model = new EmpleadosViewModel { Q = q, Numero = numero, Estatus = estatus };

        var query = _db.FiltrarEmpleados(q, numero, estatus);
        model.TotalEmpleados = await query.CountAsync();
        model.TotalPaginas = Math.Max(1, (int)Math.Ceiling((double)model.TotalEmpleados / PageSize));
        model.Pagina = Math.Clamp(pagina, 1, model.TotalPaginas);
        model.Empleados = (await query.OrderByDescending(u => u.Id).Skip((model.Pagina - 1) * PageSize)
            .Take(PageSize).ToListAsync()).Select(u => new EmpleadoFilaViewModel
            {
                Id = u.Id,
                Nombres = u.Nombres,
                ApellidoPaterno = u.ApellidoPaterno,
                ApellidoMaterno = u.ApellidoMaterno,
                CorreoEmpresarial = u.CorreoEmpresarial,
                Estatus = u.Estatus,
                FechaRegistro = u.FechaRegistro
            }).ToList();
        return model;
    }

    public async Task<EmpleadoFormViewModel> PrepararFormularioAsync(EmpleadoFormViewModel? model = null)
    {
        model ??= new EmpleadoFormViewModel();
        model.Estados = (await _db.Estados.Query.Where(e => e.Estatus == "ACTIVO" || e.Id == model.EstadoId)
            .OrderBy(e => e.NombreEstado).ToListAsync()).Select(e => new EstadoEmpleadoOpcionViewModel
            {
                Id = e.Id,
                NombreEstado = e.NombreEstado
            }).ToList();
        model.Sexos = (await _db.Sexos.Query.OrderBy(s => s.Descripcion).ToListAsync())
            .Where(s => s.Estatus == "ACTIVO" || s.Id == model.SexoId)
            .Select(s => new SexoEmpleadoOpcionViewModel { Id = s.Id, Descripcion = s.Descripcion }).ToList();
        return model;
    }

    public async Task<IReadOnlyList<(string Campo, string Mensaje)>> ValidarAsync(EmpleadoFormViewModel model, int? empleadoId = null)
    {
        var existente = empleadoId.HasValue ? await _db.Usuarios.FindAsync(empleadoId.Value) : null;
        var errores = new List<(string Campo, string Mensaje)>();
        var correo = model.CorreoEmpresarial?.Trim();
        if (await _db.Usuarios.Query.AnyAsync(u => u.Id != model.Id && u.CorreoEmpresarial == correo))
            errores.Add((nameof(model.CorreoEmpresarial), "El correo institucional ya está registrado."));
        if (model.SexoId is not char codigo || (await _db.Sexos.FindAsync(codigo)) is not { } sexo ||
            (sexo.Estatus != "ACTIVO" && existente?.SexoId != codigo))
            errores.Add((nameof(model.SexoId), "Selecciona un sexo activo."));
        var estado = await _db.Estados.FindAsync(model.EstadoId);
        if (estado is null || (estado.Estatus != "ACTIVO" && existente?.Direcciones.FirstOrDefault()?.EstadoId != model.EstadoId))
            errores.Add((nameof(model.EstadoId), "Selecciona un estado activo."));
        var anteriores = existente?.CursosUsuarios.ToArray() ?? Array.Empty<CursoUsuario>();
        var indices = new HashSet<int>();
        for (var i = 0; i < model.Cursos.Count; i++)
        {
            var curso = model.Cursos[i];
            var anterior = CursoOriginal(curso, anteriores);
            if (curso.IndiceOriginal is int indice && (anterior is null || !indices.Add(indice)))
                errores.Add((nameof(model.Cursos), "Vuelve a abrir el empleado para revisar sus cursos."));
            if (anterior is not null && CursoSinCambios(curso, anterior)) continue;
            if (curso.DuracionMinutos is null)
                errores.Add(($"Cursos[{i}].DuracionMinutos", "Indica la duración del curso."));
            if (curso.FechaInicio is null && curso.FechaEntregaConstancia is null)
                errores.Add(($"Cursos[{i}].FechaInicio", "Indica la fecha de inicio o de entrega de constancia de cada curso."));
        }
        return errores;
    }

    private static CursoUsuario? CursoOriginal(CursoEmpleadoFormViewModel curso, CursoUsuario[] anteriores) =>
        curso.IndiceOriginal is int i && i >= 0 && i < anteriores.Length ? anteriores[i] : null;

    private static bool CursoSinCambios(CursoEmpleadoFormViewModel curso, CursoUsuario anterior) =>
        anterior.NombreCurso == curso.NombreCurso?.Trim() && anterior.DuracionMinutos == curso.DuracionMinutos &&
        anterior.FechaInicio?.Date == curso.FechaInicio &&
        (anterior.FechaEntregaConstancia ?? anterior.FechaFinalizacion)?.Date == curso.FechaEntregaConstancia;

    private static void AplicarFormulario(Usuario usuario, EmpleadoFormViewModel model)
    {
        usuario.Nombres = model.Nombres.Trim();
        usuario.ApellidoPaterno = model.ApellidoPaterno.Trim();
        usuario.ApellidoMaterno = model.ApellidoMaterno?.Trim();
        usuario.FechaNacimiento = model.FechaNacimiento!.Value;
        usuario.FechaRegistro = model.FechaIngreso!.Value;
        usuario.SexoId = model.SexoId!.Value;
        usuario.CorreoEmpresarial = model.CorreoEmpresarial.Trim();
        usuario.Telefono = model.Telefono?.Trim();

        var direccion = usuario.Direcciones.FirstOrDefault();
        if (direccion is null)
        {
            direccion = new DireccionUsuario();
            usuario.Direcciones.Add(direccion);
        }
        direccion.EstadoId = model.EstadoId;
        direccion.Calle = model.Calle.Trim();
        direccion.NumeroExterior = model.NumeroExterior.Trim();
        direccion.NumeroInterior = model.NumeroInterior?.Trim();
        direccion.CodigoPostal = model.CodigoPostal.Trim();

        var anteriores = usuario.CursosUsuarios.ToArray();
        usuario.CursosUsuarios = model.Cursos.Select(c =>
        {
            var anterior = CursoOriginal(c, anteriores);
            if (anterior is not null && CursoSinCambios(c, anterior)) return anterior;
            return new CursoUsuario
            {
                CursoId = anterior?.CursoId,
                Institucion = anterior?.Institucion,
                NombreCurso = c.NombreCurso.Trim(),
                DuracionMinutos = c.DuracionMinutos,
                FechaInicio = c.FechaInicio,
                FechaEntregaConstancia = c.FechaEntregaConstancia,
                FechaFinalizacion = c.FechaEntregaConstancia,
                Estatus = c.FechaEntregaConstancia.HasValue ? "COMPLETADO" : "EN CURSO"
            };
        }).ToList();
    }
}
