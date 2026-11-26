using System.Security.Cryptography;
using System.Text;
using MongoDB.Driver.Linq;
using NoSqlU.Api.Models.ViewModels;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Models;

public sealed class AdminModel
{
    private const int PageSize = 5;
    private readonly SaturnoStore _db;

    public AdminModel(SaturnoStore db) => _db = db;

    internal async Task<DashboardViewModel> IndexAsync(int pagina = 1)
    {
        var model = new DashboardViewModel
        {
            TotalCursos = await _db.Cursos.Query.CountAsync(),
            TotalCategorias = await _db.Categorias.Query.CountAsync(),
            CursosActivos = await _db.Cursos.Query.CountAsync(c => c.Estatus == "activo")
        };
        model.TotalCursosRegistrados = model.TotalCursos;
        model.TotalPaginas = Math.Max(1, (int)Math.Ceiling((double)model.TotalCursosRegistrados / PageSize));
        model.Pagina = Math.Clamp(pagina, 1, model.TotalPaginas);
        model.Cursos = (await _db.CargarCursosAsync(pagina: model.Pagina, tamanoPagina: PageSize)).Select(CatalogoDto.From).ToList();

        return model;
    }

    internal async Task<CategoriasViewModel> CategoriasAsync(int pagina = 1)
    {
        return await BuildCategoriasViewModelAsync(pagina: pagina);
    }

    internal async Task<AdminResultado> CrearCategoriaAsync(CategoriaCreateViewModel nuevaCategoria, bool formularioValido, int pagina)
    {
        var errores = new List<ErrorFormulario>();
        if (!formularioValido || errores.Count > 0)
            return AdminResultado.Invalido(await BuildCategoriasViewModelAsync(nuevaCategoria, pagina), errores);

        var nombre = nuevaCategoria.Nombre.Trim();
        if (await _db.Categorias.Query.AnyAsync(c => c.Nombre == nombre))
        {
            errores.Add(new ErrorFormulario(nameof(nuevaCategoria.Nombre), "Ya existe una categoría con ese nombre."));
            return AdminResultado.Invalido(await BuildCategoriasViewModelAsync(nuevaCategoria, pagina), errores);
        }

        await _db.Categorias.InsertAsync(new Categoria
        {
            Nombre = nombre,
            Descripcion = nuevaCategoria.Descripcion?.Trim(),
            Estatus = "activo",
            FechaCreacion = DateTime.UtcNow
        });
        return AdminResultado.Guardado();
    }

    internal async Task<AdminResultado> CambiarCategoriaEstatusAsync(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null) return AdminResultado.NoEncontrado();

        categoria.Estatus = categoria.Estatus == "activo" ? "inactivo" : "activo";
        await _db.Categorias.ReplaceAsync(categoria);
        return AdminResultado.Guardado();
    }

    internal async Task<CursoCreateViewModel> CursosAsync()
    {
        return await BuildCursoViewModelAsync(new CursoCreateViewModel());
    }

    internal async Task<AdminResultado> CrearCursoAsync(CursoCreateViewModel model, bool formularioValido)
    {
        var errores = new List<ErrorFormulario>();
        if (!formularioValido || errores.Count > 0)
            return AdminResultado.Invalido(await BuildCursoViewModelAsync(model), errores);

        var categoria = await _db.Categorias.Query
            .FirstOrDefaultAsync(c => c.Id == model.CategoriaId && c.Estatus == "activo");
        if (categoria is null)
        {
            errores.Add(new ErrorFormulario(nameof(model.CategoriaId), "La categoría seleccionada no existe o está inactiva."));
            return AdminResultado.Invalido(await BuildCursoViewModelAsync(model), errores);
        }

        var nombreNivel = model.Nivel.Trim();
        var nivel = await _db.Niveles.Query.FirstOrDefaultAsync(n => n.Nombre == nombreNivel);
        if (nivel is null)
        {
            nivel = new Nivel { Nombre = nombreNivel, Estatus = "activo", FechaCreacion = DateTime.UtcNow };
            await _db.Niveles.InsertAsync(nivel);
        }
        else
        {
            nivel.Estatus = "activo";
            await _db.Niveles.ReplaceAsync(nivel);
        }

        var nombreInstructor = model.Instructor.Trim();
        var instructor = await _db.Instructores.Query.FirstOrDefaultAsync(i => i.Nombre == nombreInstructor);
        if (instructor is null)
        {
            instructor = new Instructor
            {
                Nombre = nombreInstructor,
                Apellido = string.Empty,
                Email = string.Empty,
                Estatus = "activo",
                FechaCreacion = DateTime.UtcNow
            };
            await _db.Instructores.InsertAsync(instructor);
        }
        else
        {
            instructor.Estatus = "activo";
            await _db.Instructores.ReplaceAsync(instructor);
        }

        await _db.Cursos.InsertAsync(new Curso
        {
            Nombre = model.Nombre.Trim(),
            Descripcion = model.Descripcion.Trim(),
            Precio = model.Precio,
            DuracionMinutos = model.DuracionMinutos,
            FechaPublicacion = model.FechaPublicacion!.Value,
            CategoriaId = categoria.Id,
            NivelId = nivel.Id,
            InstructorId = instructor.Id,
            Estatus = "activo",
            FechaCreacion = DateTime.UtcNow
        });
        return AdminResultado.Guardado();
    }

    internal async Task<AdminResultado> CambiarCursoEstatusAsync(int id)
    {
        var curso = await _db.Cursos.FindAsync(id);
        if (curso is null) return AdminResultado.NoEncontrado();

        curso.Estatus = curso.Estatus == "activo" ? "inactivo" : "activo";
        await _db.Cursos.ReplaceAsync(curso);
        return AdminResultado.Guardado();
    }

    internal async Task<BuscarCursosViewModel> BuscarAsync(BuscarCursosViewModel model, int pagina = 1)
    {
        var query = _db.Cursos.Query;

        if (!string.IsNullOrWhiteSpace(model.Q))
            query = query.Where(c => c.Nombre.Contains(model.Q) || c.Descripcion.Contains(model.Q));
        if (!string.IsNullOrWhiteSpace(model.Categoria))
        {
            var categoriaId = await _db.Categorias.Query
                .Where(c => c.Nombre == model.Categoria)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();
            query = query.Where(c => c.CategoriaId == categoriaId);
        }
        if (!string.IsNullOrWhiteSpace(model.Nivel))
        {
            var nivelId = await _db.Niveles.Query
                .Where(n => n.Nombre == model.Nivel)
                .Select(n => n.Id)
                .FirstOrDefaultAsync();
            query = query.Where(c => c.NivelId == nivelId);
        }
        if (!string.IsNullOrWhiteSpace(model.Instructor))
        {
            var instructorId = await _db.Instructores.Query
                .Where(i => i.Nombre == model.Instructor)
                .Select(i => i.Id)
                .FirstOrDefaultAsync();
            query = query.Where(c => c.InstructorId == instructorId);
        }
        if (!string.IsNullOrWhiteSpace(model.Estatus))
            query = query.Where(c => c.Estatus == model.Estatus);
        if (model.Min.HasValue)
            query = query.Where(c => c.Precio >= model.Min.Value);
        if (model.Max.HasValue)
            query = query.Where(c => c.Precio <= model.Max.Value);

        model.Categorias = (await _db.Categorias.Query.Where(c => c.Estatus == "activo").OrderBy(c => c.Nombre).ToListAsync()).Select(CatalogoDto.From).ToList();
        model.Niveles = (await _db.Niveles.Query.Where(n => n.Estatus == "activo").OrderBy(n => n.Nombre).ToListAsync()).Select(CatalogoDto.From).ToList();
        model.Instructores = (await _db.Instructores.Query.Where(i => i.Estatus == "activo").OrderBy(i => i.Nombre).ToListAsync()).Select(CatalogoDto.From).ToList();
        model.TotalResultados = await query.CountAsync();
        model.TotalPaginas = Math.Max(1, (int)Math.Ceiling((double)model.TotalResultados / PageSize));
        model.Pagina = Math.Clamp(pagina, 1, model.TotalPaginas);
        model.Resultados = (await _db.CargarCursosAsync(query, model.Pagina, PageSize)).Select(CatalogoDto.From).ToList();
        return model;
    }

    internal async Task<EmpleadosViewModel> EmpleadosAsync(string? q, string? estatus, int pagina = 1, int? numero = null)
    {
        return await BuildEmpleadosViewModelAsync(q, estatus, pagina, numero);
    }

    internal async Task<EmpleadoFormViewModel> RegistrarEmpleadoAsync() => await BuildEmpleadoFormViewModelAsync();



    internal async Task<AdminResultado> RegistrarEmpleadoAsync(EmpleadoFormViewModel model, bool formularioValido)
    {
        var errores = new List<ErrorFormulario>();
        await ValidarEmpleadoAsync(model, errores);
        if (!formularioValido || errores.Count > 0)
            return AdminResultado.Invalido(await BuildEmpleadoFormViewModelAsync(model), errores);

        var usuario = new Usuario { Estatus = "ACTIVO" };
        AplicarFormulario(usuario, model);
        await _db.Usuarios.InsertAsync(usuario);
        return AdminResultado.Guardado(usuario.Id);
    }

    internal async Task<EmpleadoFormViewModel?> EditarEmpleadoAsync(int id)
    {
        var usuario = await _db.Usuarios.Query
            .FirstOrDefaultAsync(u => u.Id == id);
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
        return await BuildEmpleadoFormViewModelAsync(model);
    }

    internal async Task<AdminResultado> EditarEmpleadoAsync(EmpleadoFormViewModel model, bool formularioValido)
    {
        var errores = new List<ErrorFormulario>();
        var usuario = await _db.Usuarios.Query
            .FirstOrDefaultAsync(u => u.Id == model.Id);
        if (usuario is null) return AdminResultado.NoEncontrado();

        await ValidarEmpleadoAsync(model, errores, usuario);
        if (!formularioValido || errores.Count > 0)
            return AdminResultado.Invalido(await BuildEmpleadoFormViewModelAsync(model), errores);

        AplicarFormulario(usuario, model);
        usuario.Estatus = model.Estatus;
        await _db.EditarEmpleadoAsync(usuario);
        return AdminResultado.Guardado(usuario.Id);
    }

    internal async Task<AdminResultado> EliminarEmpleadoAsync(int id)
    {
        if (!await _db.EliminarEmpleadoInactivoAsync(id))
            return AdminResultado.Conflicto("Solo se puede eliminar definitivamente a un empleado inactivo.");
        return AdminResultado.Guardado();
    }



    private async Task ValidarEmpleadoAsync(EmpleadoFormViewModel model, List<ErrorFormulario> errores, Usuario? existente = null)
    {
        var correo = model.CorreoEmpresarial?.Trim();
        if (await _db.Usuarios.Query.AnyAsync(u => u.Id != model.Id && u.CorreoEmpresarial == correo))
            errores.Add(new ErrorFormulario(nameof(model.CorreoEmpresarial), "El correo institucional ya está registrado."));
        if (model.SexoId is not char codigo || (await _db.Sexos.FindAsync(codigo)) is not { } sexo ||
            (sexo.Estatus != "ACTIVO" && existente?.SexoId != codigo))
            errores.Add(new ErrorFormulario(nameof(model.SexoId), "Selecciona un sexo activo."));
        var estado = await _db.Estados.FindAsync(model.EstadoId);
        if (estado is null || (estado.Estatus != "ACTIVO" && existente?.Direcciones.FirstOrDefault()?.EstadoId != model.EstadoId))
            errores.Add(new ErrorFormulario(nameof(model.EstadoId), "Selecciona un estado activo."));
        var anteriores = existente?.CursosUsuarios.ToArray() ?? Array.Empty<CursoUsuario>();
        var indices = new HashSet<int>();
        for (var i = 0; i < model.Cursos.Count; i++)
        {
            var curso = model.Cursos[i];
            var anterior = CursoOriginal(curso, anteriores);
            if (curso.IndiceOriginal is int indice && (anterior is null || !indices.Add(indice)))
                errores.Add(new ErrorFormulario(nameof(model.Cursos), "Vuelve a abrir el empleado para revisar sus cursos."));
            if (anterior is not null && CursoSinCambios(curso, anterior)) continue;
            if (curso.DuracionMinutos is null)
                errores.Add(new ErrorFormulario($"Cursos[{i}].DuracionMinutos", "Indica la duración del curso."));
            if (curso.FechaInicio is null && curso.FechaEntregaConstancia is null)
                errores.Add(new ErrorFormulario($"Cursos[{i}].FechaInicio", "Indica la fecha de inicio o de entrega de constancia de cada curso."));
        }
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

    internal async Task<CatalogosViewModel> CatalogosAsync(string tipo = "sexos", string? editar = null, int pagina = 1)
    {
        return await BuildCatalogosViewModelAsync(tipo, editar, pagina: pagina, mostrarFormulario: editar is not null);
    }

    internal async Task<AdminResultado> GuardarCatalogoAsync(
        CatalogoElementoViewModel elemento,
        string tipo,
        string? claveOriginal,
        bool formularioValido,
        int pagina)
    {
        var errores = new List<ErrorFormulario>();
        if (!EsTipoCatalogoValido(tipo)) return AdminResultado.NoEncontrado();

        if (!string.IsNullOrWhiteSpace(claveOriginal) &&
            (tipo == "sexos" ? !char.TryParse(claveOriginal, out _) : !int.TryParse(claveOriginal, out _)))
            return AdminResultado.DatosInvalidos();

        if (tipo == "sexos" &&
            (string.IsNullOrWhiteSpace(elemento.Clave) || elemento.Clave.Trim().Length != 1))
            errores.Add(new ErrorFormulario("Elemento.Clave", "La clave del sexo debe ser un solo carácter."));

        if (formularioValido && errores.Count == 0)
        {
            var duplicado = await ExisteCatalogoDuplicadoAsync(tipo, elemento.Nombre.Trim(), elemento.Clave?.Trim(), claveOriginal);
            if (duplicado)
                errores.Add(new ErrorFormulario("Elemento.Nombre", "Ya existe un elemento con ese nombre o clave."));
        }

        if (!formularioValido || errores.Count > 0)
            return AdminResultado.Invalido(await BuildCatalogosViewModelAsync(tipo, claveOriginal, elemento, mostrarFormulario: true, pagina: pagina), errores);

        var editar = !string.IsNullOrWhiteSpace(claveOriginal);
        switch (tipo)
        {
            case "sexos":
                var claveSexo = elemento.Clave!.Trim().ToUpperInvariant()[0];
                var sexo = editar
                    ? await _db.Sexos.FindAsync(char.Parse(claveOriginal!))
                    : new Sexo { Id = claveSexo };
                if (sexo is null) return AdminResultado.NoEncontrado();
                if (editar && sexo.Id != claveSexo)
                {
                    errores.Add(new ErrorFormulario("Elemento.Clave", "No es posible cambiar la clave de un sexo existente."));
                    return AdminResultado.Invalido(await BuildCatalogosViewModelAsync(tipo, claveOriginal, elemento, mostrarFormulario: true, pagina: pagina), errores);
                }
                sexo.Descripcion = elemento.Nombre.Trim();

                if (editar) await _db.Sexos.ReplaceAsync(sexo);
                else await _db.Sexos.InsertAsync(sexo);
                break;
            case "estados":
                var estado = editar
                    ? await _db.Estados.Query.FirstOrDefaultAsync(e => e.Id == int.Parse(claveOriginal!))
                    : new Estado();
                if (estado is null) return AdminResultado.NoEncontrado();
                estado.NombreEstado = elemento.Nombre.Trim();

                if (editar) await _db.Estados.ReplaceAsync(estado);
                else await _db.Estados.InsertAsync(estado);
                break;
        }


        return AdminResultado.Guardado();
    }

    internal async Task<AdminResultado> EliminarCatalogoAsync(string tipo, string clave)
    {
        if (!EsTipoCatalogoValido(tipo)) return AdminResultado.NoEncontrado();
        if (tipo == "sexos" ? !char.TryParse(clave, out _) : !int.TryParse(clave, out _))
            return AdminResultado.DatosInvalidos();
        switch (tipo)
        {
            case "sexos":
                var sexo = await _db.Sexos.FindAsync(char.Parse(clave));
                if (sexo is null) return AdminResultado.NoEncontrado();
                if (sexo.Estatus == "ACTIVO" && await _db.SexoEnUsoAsync(sexo.Id))
                    return AdminResultado.Conflicto("Este sexo está en uso.");
                sexo.Estatus = sexo.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
                await _db.Sexos.ReplaceAsync(sexo);
                break;
            case "estados":
                var estado = await _db.Estados.FindAsync(int.Parse(clave));
                if (estado is null) return AdminResultado.NoEncontrado();
                if (estado.Estatus == "ACTIVO" && await _db.EstadoEnUsoAsync(estado.Id))
                    return AdminResultado.Conflicto("Este estado está en uso.");
                estado.Estatus = estado.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
                await _db.Estados.ReplaceAsync(estado);
                break;
        }

        return AdminResultado.Guardado();
    }

    internal async Task<EmpleadoDetalleViewModel?> DetalleUsuarioAsync(int id)
    {
        var usuario = await _db.BuscarDetalleEmpleadoAsync(id);

        return usuario is null ? null : EmpleadoDetalleViewModel.From(usuario);
    }

    internal async Task<AdminResultado> RegistrarAsync(RegistroUsuarioViewModel model, bool formularioValido)
    {
        var errores = new List<ErrorFormulario>();
        if (string.IsNullOrWhiteSpace(model.ApellidoPaterno) && string.IsNullOrWhiteSpace(model.ApellidoMaterno))
            errores.Add(new ErrorFormulario(nameof(model.ApellidoPaterno), "Debes proporcionar al menos un apellido."));

        if (await _db.Usuarios.Query.AnyAsync(u => u.CorreoEmpresarial == model.CorreoEmpresarial))
            errores.Add(new ErrorFormulario(nameof(model.CorreoEmpresarial), "El correo empresarial ya está registrado."));
        if (await _db.LoginExistsAsync(model.UsuarioLogin))
            errores.Add(new ErrorFormulario(nameof(model.UsuarioLogin), "El nombre de usuario ya existe."));

        if (!formularioValido || errores.Count > 0)
            return AdminResultado.Invalido(await BuildRegistroViewModelAsync(model), errores);

        using var sha256 = SHA256.Create();
        var passwordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(model.Password)));
        var usuario = new Usuario
        {
            Nombres = model.Nombres.Trim(),
            ApellidoPaterno = model.ApellidoPaterno?.Trim(),
            ApellidoMaterno = model.ApellidoMaterno?.Trim(),
            FechaNacimiento = model.FechaNacimiento!.Value,
            SexoId = model.SexoId!.Value,
            CorreoEmpresarial = model.CorreoEmpresarial.Trim(),
            Telefono = model.Telefono?.Trim(),
            Estatus = "ACTIVO",
            Contrasena = new UsuarioContrasena
            {
                UsuarioLogin = model.UsuarioLogin.Trim(),
                PasswordHash = passwordHash
            },
            Direcciones = new List<DireccionUsuario>
            {
                new DireccionUsuario
                {
                    EstadoId = model.EstadoId,
                    Calle = model.Calle.Trim(),
                    NumeroExterior = model.NumeroExterior.Trim(),
                    NumeroInterior = model.NumeroInterior?.Trim(),
                    CodigoPostal = model.CodigoPostal.Trim(),
                    Colonia = model.Colonia.Trim(),
                    Municipio = model.Municipio.Trim()
                }
            }
        };

        await _db.Usuarios.InsertAsync(usuario);
        return AdminResultado.Guardado(usuario.Id);
    }

    internal async Task<AdminResultado> CambiarUsuarioEstatusAsync(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return AdminResultado.NoEncontrado();

        usuario.Estatus = "INACTIVO";
        await _db.CambiarEstatusEmpleadoAsync(usuario);
        return AdminResultado.Guardado();
    }

    internal async Task<AsignarCursoViewModel?> AsignarCursoAsync(int id)
    {
        var usuario = await _db.Usuarios.Query.FirstOrDefaultAsync(u => u.Id == id);
        if (usuario is null) return null;

        return await BuildAsignarCursoViewModelAsync(usuario.Id, usuario.Nombres, usuario.CorreoEmpresarial);
    }

    internal async Task<AdminResultado> AsignarCursoAsync(AsignarCursoViewModel model, bool formularioValido)
    {
        var errores = new List<ErrorFormulario>();
        var usuario = await _db.Usuarios.FindAsync(model.UsuarioId);
        if (usuario is null) return AdminResultado.NoEncontrado();

        if (!formularioValido || errores.Count > 0)
            return AdminResultado.Invalido(await BuildAsignarCursoViewModelAsync(usuario.Id, usuario.Nombres, usuario.CorreoEmpresarial, model.CursoId), errores);
        var curso = await _db.Cursos.FindAsync(model.CursoId);
        if (curso?.Estatus != "activo")
        {
            errores.Add(new ErrorFormulario(nameof(model.CursoId), "El curso seleccionado no existe o está inactivo."));
            return AdminResultado.Invalido(await BuildAsignarCursoViewModelAsync(usuario.Id, usuario.Nombres, usuario.CorreoEmpresarial, model.CursoId), errores);
        }
        if (await _db.TieneCursoAsync(model.UsuarioId, model.CursoId))
        {
            errores.Add(new ErrorFormulario(nameof(model.CursoId), "El usuario ya está inscrito en este curso."));
            return AdminResultado.Invalido(await BuildAsignarCursoViewModelAsync(usuario.Id, usuario.Nombres, usuario.CorreoEmpresarial, model.CursoId), errores);
        }

        if (!await _db.AsignarCursoAsync(model.UsuarioId, new CursoUsuario
        {
            CursoId = model.CursoId,
            NombreCurso = curso.Nombre,
            DuracionMinutos = curso.DuracionMinutos,
            FechaInicio = DateTime.UtcNow,
            Estatus = "EN CURSO"
        }))
            return AdminResultado.Conflicto("La inscripción ya existe o el empleado fue eliminado.");
        return AdminResultado.Guardado();
    }



    private async Task<CategoriasViewModel> BuildCategoriasViewModelAsync(
        CategoriaCreateViewModel? nueva = null, int pagina = 1)
    {
        var query = _db.Categorias.Query.OrderBy(c => c.Nombre);
        var model = new CategoriasViewModel
        {
            NuevaCategoria = nueva ?? new CategoriaCreateViewModel()
        };
        model.TotalCategorias = await query.CountAsync();
        model.TotalPaginas = Math.Max(1, (int)Math.Ceiling((double)model.TotalCategorias / PageSize));
        model.Pagina = Math.Clamp(pagina, 1, model.TotalPaginas);
        model.Categorias = (await query.Skip((model.Pagina - 1) * PageSize).Take(PageSize).ToListAsync()).Select(CatalogoDto.From).ToList();
        return model;
    }

    private async Task<CursoCreateViewModel> BuildCursoViewModelAsync(CursoCreateViewModel model)
    {
        model.Categorias = (await _db.Categorias.Query.Where(c => c.Estatus == "activo").OrderBy(c => c.Nombre).ToListAsync()).Select(CatalogoDto.From).ToList();
        model.Niveles = (await _db.Niveles.Query.Where(n => n.Estatus == "activo").OrderBy(n => n.Nombre).ToListAsync()).Select(CatalogoDto.From).ToList();
        model.Instructores = (await _db.Instructores.Query.Where(i => i.Estatus == "activo").OrderBy(i => i.Nombre).ToListAsync()).Select(CatalogoDto.From).ToList();
        return model;
    }

    private async Task<RegistroUsuarioViewModel> BuildRegistroViewModelAsync(RegistroUsuarioViewModel model)
    {
        model.Estados = (await _db.Estados.Query.Where(e => e.Estatus == "ACTIVO").OrderBy(e => e.NombreEstado).ToListAsync()).Select(CatalogoDto.From).ToList();
        model.Sexos = (await _db.Sexos.Query.Where(s => s.Estatus == "ACTIVO").OrderBy(s => s.Descripcion).ToListAsync()).Select(CatalogoDto.From).ToList();
        return model;
    }

    private async Task<AsignarCursoViewModel> BuildAsignarCursoViewModelAsync(
        int usuarioId, string nombre, string correo, int cursoId = 0)
    {
        return new AsignarCursoViewModel
        {
            UsuarioId = usuarioId,
            NombreUsuario = nombre,
            CorreoUsuario = correo,
            CursoId = cursoId,
            Cursos = (await _db.Cursos.Query
                .Where(c => c.Estatus == "activo")
                .OrderBy(c => c.Nombre)
                .ToListAsync()).Select(CatalogoDto.From).ToList()
        };
    }

    private async Task<EmpleadosViewModel> BuildEmpleadosViewModelAsync(string? q, string? estatus, int pagina, int? numero)
    {
        var model = new EmpleadosViewModel { Q = q, Numero = numero, Estatus = estatus };

        var query = _db.FiltrarEmpleados(q, numero, estatus);
        model.TotalEmpleados = await query.CountAsync();
        model.TotalPaginas = Math.Max(1, (int)Math.Ceiling((double)model.TotalEmpleados / PageSize));
        model.Pagina = Math.Clamp(pagina, 1, model.TotalPaginas);
        model.Empleados = await query.OrderByDescending(u => u.Id).Skip((model.Pagina - 1) * PageSize)
            .Take(PageSize)
            .Select(u => new EmpleadoFilaViewModel(u.Id, u.Nombres, u.ApellidoPaterno, u.ApellidoMaterno,
                u.CorreoEmpresarial, u.FechaRegistro, u.Estatus)).ToListAsync();
        return model;
    }

    private async Task<EmpleadoFormViewModel> BuildEmpleadoFormViewModelAsync(EmpleadoFormViewModel? model = null)
    {
        model ??= new EmpleadoFormViewModel();
        model.Estados = (await _db.Estados.Query.Where(e => e.Estatus == "ACTIVO" || e.Id == model.EstadoId)
            .OrderBy(e => e.NombreEstado).ToListAsync()).Select(CatalogoDto.From).ToList();
        model.Sexos = (await _db.Sexos.Query.OrderBy(s => s.Descripcion).ToListAsync())
            .Where(s => s.Estatus == "ACTIVO" || s.Id == model.SexoId).Select(CatalogoDto.From).ToList();
        return model;
    }

    private async Task<CatalogosViewModel> BuildCatalogosViewModelAsync(
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

    private async Task<bool> ExisteCatalogoDuplicadoAsync(string tipo, string nombre, string? clave, string? claveOriginal)
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

    internal static bool EsTipoCatalogoValido(string tipo) =>
        tipo is "sexos" or "estados";
}
