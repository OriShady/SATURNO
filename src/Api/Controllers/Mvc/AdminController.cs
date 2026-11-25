using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;
using NoSqlU.Api.Models.ViewModels;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Controllers.Mvc;

public class AdminController : Controller
{
    private readonly SaturnoStore _db;

    public AdminController(SaturnoStore db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            TotalCursos = await _db.Cursos.Query.CountAsync(),
            TotalCategorias = await _db.Categorias.Query.CountAsync(),
            CursosActivos = await _db.Cursos.Query.CountAsync(c => c.Estatus == "activo"),
            Cursos = (await _db.CargarCursosAsync()).OrderByDescending(c => c.FechaCreacion).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Categorias()
    {
        return View(await BuildCategoriasViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(CategoriaCreateViewModel nuevaCategoria)
    {
        if (!ModelState.IsValid)
            return View("Categorias", await BuildCategoriasViewModelAsync(nuevaCategoria));

        var nombre = nuevaCategoria.Nombre.Trim();
        if (await _db.Categorias.Query.AnyAsync(c => c.Nombre == nombre))
        {
            ModelState.AddModelError(nameof(nuevaCategoria.Nombre), "Ya existe una categoría con ese nombre.");
            return View("Categorias", await BuildCategoriasViewModelAsync(nuevaCategoria));
        }

        await _db.Categorias.InsertAsync(new Categoria
        {
            Nombre = nombre,
            Descripcion = nuevaCategoria.Descripcion?.Trim(),
            Estatus = "activo",
            FechaCreacion = DateTime.UtcNow
        });
        TempData["Mensaje"] = "Categoría creada correctamente.";
        return RedirectToAction(nameof(Categorias));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarCategoriaEstatus(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null) return NotFound();

        categoria.Estatus = categoria.Estatus == "activo" ? "inactivo" : "activo";
        await _db.Categorias.ReplaceAsync(categoria);
        return RedirectToAction(nameof(Categorias));
    }

    [HttpGet]
    public async Task<IActionResult> Cursos()
    {
        return View(await BuildCursoViewModelAsync(new CursoCreateViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCurso(CursoCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Cursos", await BuildCursoViewModelAsync(model));

        var categoria = await _db.Categorias.Query
            .FirstOrDefaultAsync(c => c.Id == model.CategoriaId && c.Estatus == "activo");
        if (categoria is null)
        {
            ModelState.AddModelError(nameof(model.CategoriaId), "La categoría seleccionada no existe o está inactiva.");
            return View("Cursos", await BuildCursoViewModelAsync(model));
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
        TempData["Mensaje"] = "Curso creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarCursoEstatus(int id)
    {
        var curso = await _db.Cursos.FindAsync(id);
        if (curso is null) return NotFound();

        curso.Estatus = curso.Estatus == "activo" ? "inactivo" : "activo";
        await _db.Cursos.ReplaceAsync(curso);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Buscar(BuscarCursosViewModel model)
    {
        var query = (await _db.CargarCursosAsync()).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(model.Q))
            query = query.Where(c => c.Nombre.Contains(model.Q) || c.Descripcion.Contains(model.Q));
        if (!string.IsNullOrWhiteSpace(model.Categoria))
            query = query.Where(c => c.Categoria?.Nombre == model.Categoria);
        if (!string.IsNullOrWhiteSpace(model.Nivel))
            query = query.Where(c => c.Nivel?.Nombre == model.Nivel);
        if (!string.IsNullOrWhiteSpace(model.Instructor))
            query = query.Where(c => c.Instructor?.Nombre == model.Instructor);
        if (!string.IsNullOrWhiteSpace(model.Estatus))
            query = query.Where(c => c.Estatus == model.Estatus);
        if (model.Min.HasValue)
            query = query.Where(c => c.Precio >= model.Min.Value);
        if (model.Max.HasValue)
            query = query.Where(c => c.Precio <= model.Max.Value);

        model.Categorias = await _db.Categorias.Query.Where(c => c.Estatus == "activo").OrderBy(c => c.Nombre).ToListAsync();
        model.Niveles = await _db.Niveles.Query.Where(n => n.Estatus == "activo").OrderBy(n => n.Nombre).ToListAsync();
        model.Instructores = await _db.Instructores.Query.Where(i => i.Estatus == "activo").OrderBy(i => i.Nombre).ToListAsync();
        model.Resultados = query.OrderBy(c => c.Nombre).ToList();
        return View(model);
    }

    [HttpGet]
    public IActionResult Usuarios() => RedirectToAction(nameof(Empleados));

    [HttpGet]
    public async Task<IActionResult> Empleados(string? q, string? estatus, int pagina = 1, int? numero = null)
    {
        if (estatus is not (null or "ACTIVO" or "INACTIVO") || numero is <= 0)
            return BadRequest("Revisa el número de empleado y el estatus.");
        return View(await BuildEmpleadosViewModelAsync(q, estatus, pagina, numero));
    }

    [HttpGet]
    public async Task<IActionResult> RegistrarEmpleado() => View("FormularioEmpleado", await BuildEmpleadoFormViewModelAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarEmpleado(EmpleadoFormViewModel model)
    {
        model.Id = 0;
        ModelState.Remove(nameof(model.Id));
        await ValidarEmpleadoAsync(model);
        if (!ModelState.IsValid)
            return View("FormularioEmpleado", await BuildEmpleadoFormViewModelAsync(model));

        var usuario = new Usuario { Estatus = "ACTIVO" };
        AplicarFormulario(usuario, model);
        await _db.Usuarios.InsertAsync(usuario);
        TempData["Mensaje"] = $"Empleado registrado correctamente (No. {usuario.Id}).";
        return RedirectToAction(nameof(Empleados), new { numero = usuario.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditarEmpleado(int id)
    {
        var usuario = await _db.Usuarios.Query
            .FirstOrDefaultAsync(u => u.Id == id);
        if (usuario is null) return NotFound();

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
        return View("FormularioEmpleado", await BuildEmpleadoFormViewModelAsync(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarEmpleado(EmpleadoFormViewModel model)
    {
        var usuario = await _db.Usuarios.Query
            .FirstOrDefaultAsync(u => u.Id == model.Id);
        if (usuario is null) return NotFound();

        await ValidarEmpleadoAsync(model, usuario);
        if (!ModelState.IsValid)
            return View("FormularioEmpleado", await BuildEmpleadoFormViewModelAsync(model));

        AplicarFormulario(usuario, model);
        usuario.Estatus = model.Estatus;
        await _db.EditarEmpleadoAsync(usuario);
        TempData["Mensaje"] = "Datos del empleado actualizados correctamente.";
        return RedirectToAction(nameof(Empleados), new { numero = usuario.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarEmpleado(int id)
    {
        if (!await _db.EliminarEmpleadoInactivoAsync(id))
            return Conflict("Solo se puede eliminar definitivamente a un empleado inactivo.");
        TempData["Mensaje"] = "El empleado se eliminó definitivamente.";
        return RedirectToAction(nameof(Empleados));
    }

    private async Task ValidarEmpleadoAsync(EmpleadoFormViewModel model, Usuario? existente = null)
    {
        var correo = model.CorreoEmpresarial?.Trim();
        if (await _db.Usuarios.Query.AnyAsync(u => u.Id != model.Id && u.CorreoEmpresarial == correo))
            ModelState.AddModelError(nameof(model.CorreoEmpresarial), "El correo institucional ya está registrado.");
        if (model.SexoId is not char codigo || (await _db.Sexos.FindAsync(codigo)) is not { } sexo ||
            (sexo.Estatus != "ACTIVO" && existente?.SexoId != codigo))
            ModelState.AddModelError(nameof(model.SexoId), "Selecciona un sexo activo.");
        var estado = await _db.Estados.FindAsync(model.EstadoId);
        if (estado is null || (estado.Estatus != "ACTIVO" && existente?.Direcciones.FirstOrDefault()?.EstadoId != model.EstadoId))
            ModelState.AddModelError(nameof(model.EstadoId), "Selecciona un estado activo.");
        var anteriores = existente?.CursosUsuarios.ToArray() ?? Array.Empty<CursoUsuario>();
        var indices = new HashSet<int>();
        for (var i = 0; i < model.Cursos.Count; i++)
        {
            var curso = model.Cursos[i];
            var anterior = CursoOriginal(curso, anteriores);
            if (curso.IndiceOriginal is int indice && (anterior is null || !indices.Add(indice)))
                ModelState.AddModelError(nameof(model.Cursos), "Vuelve a abrir el empleado para revisar sus cursos.");
            if (anterior is not null && CursoSinCambios(curso, anterior)) continue;
            if (curso.DuracionMinutos is null)
                ModelState.AddModelError($"Cursos[{i}].DuracionMinutos", "Indica la duración del curso.");
            if (curso.FechaInicio is null && curso.FechaEntregaConstancia is null)
                ModelState.AddModelError($"Cursos[{i}].FechaInicio", "Indica la fecha de inicio o de entrega de constancia de cada curso.");
            if (curso.FechaInicio > curso.FechaEntregaConstancia)
                ModelState.AddModelError($"Cursos[{i}].FechaEntregaConstancia", "La entrega de constancia no puede ser anterior al inicio.");
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

    [HttpGet]
    public async Task<IActionResult> Catalogos(string tipo = "sexos", string? editar = null, int pagina = 1)
    {
        tipo = tipo.ToLowerInvariant();
        if (!EsTipoCatalogoValido(tipo)) return NotFound();
        return View(await BuildCatalogosViewModelAsync(tipo, editar, pagina: pagina, mostrarFormulario: editar is not null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarCatalogo(
        [Bind(Prefix = "Elemento")] CatalogoElementoViewModel elemento,
        string tipo,
        string? claveOriginal,
        int pagina = 1)
    {
        tipo = tipo.ToLowerInvariant();
        if (!EsTipoCatalogoValido(tipo)) return NotFound();

        if (!string.IsNullOrWhiteSpace(claveOriginal) &&
            (tipo == "sexos" ? !char.TryParse(claveOriginal, out _) : !int.TryParse(claveOriginal, out _)))
            return BadRequest();

        if (tipo == "sexos" &&
            (string.IsNullOrWhiteSpace(elemento.Clave) || elemento.Clave.Trim().Length != 1))
            ModelState.AddModelError("Elemento.Clave", "La clave del sexo debe ser un solo carácter.");

        if (ModelState.IsValid)
        {
            var duplicado = await ExisteCatalogoDuplicadoAsync(tipo, elemento.Nombre.Trim(), elemento.Clave?.Trim(), claveOriginal);
            if (duplicado)
                ModelState.AddModelError("Elemento.Nombre", "Ya existe un elemento con ese nombre o clave.");
        }

        if (!ModelState.IsValid)
            return View("Catalogos", await BuildCatalogosViewModelAsync(tipo, claveOriginal, elemento, mostrarFormulario: true, pagina: pagina));

        var editar = !string.IsNullOrWhiteSpace(claveOriginal);
        switch (tipo)
        {
            case "sexos":
                var claveSexo = elemento.Clave!.Trim().ToUpperInvariant()[0];
                var sexo = editar
                    ? await _db.Sexos.FindAsync(char.Parse(claveOriginal!))
                    : new Sexo { Id = claveSexo };
                if (sexo is null) return NotFound();
                if (editar && sexo.Id != claveSexo)
                {
                    ModelState.AddModelError("Elemento.Clave", "No es posible cambiar la clave de un sexo existente.");
                    return View("Catalogos", await BuildCatalogosViewModelAsync(tipo, claveOriginal, elemento, mostrarFormulario: true, pagina: pagina));
                }
                sexo.Descripcion = elemento.Nombre.Trim();

                if (editar) await _db.Sexos.ReplaceAsync(sexo);
                else await _db.Sexos.InsertAsync(sexo);
                break;
            case "estados":
                var estado = editar
                    ? await _db.Estados.Query.FirstOrDefaultAsync(e => e.Id == int.Parse(claveOriginal!))
                    : new Estado();
                if (estado is null) return NotFound();
                estado.NombreEstado = elemento.Nombre.Trim();

                if (editar) await _db.Estados.ReplaceAsync(estado);
                else await _db.Estados.InsertAsync(estado);
                break;
        }


        TempData["Mensaje"] = editar ? "Elemento actualizado correctamente." : "Elemento agregado correctamente.";
        return RedirectToAction(nameof(Catalogos), new { tipo, pagina });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCatalogo(string tipo, string clave, int pagina = 1)
    {
        tipo = tipo.ToLowerInvariant();
        if (!EsTipoCatalogoValido(tipo)) return NotFound();
        if (tipo == "sexos" ? !char.TryParse(clave, out _) : !int.TryParse(clave, out _))
            return BadRequest();
        switch (tipo)
        {
            case "sexos":
                var sexo = await _db.Sexos.FindAsync(char.Parse(clave));
                if (sexo is null) return NotFound();
                if (sexo.Estatus == "ACTIVO" && await _db.SexoEnUsoAsync(sexo.Id))
                    return Conflict("Este sexo está en uso.");
                sexo.Estatus = sexo.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
                await _db.Sexos.ReplaceAsync(sexo);
                break;
            case "estados":
                var estado = await _db.Estados.FindAsync(int.Parse(clave));
                if (estado is null) return NotFound();
                if (estado.Estatus == "ACTIVO" && await _db.EstadoEnUsoAsync(estado.Id))
                    return Conflict("Este estado está en uso.");
                estado.Estatus = estado.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
                await _db.Estados.ReplaceAsync(estado);
                break;
        }

        TempData["Mensaje"] = "Estatus del elemento actualizado; se conserva el historial asociado.";
        return RedirectToAction(nameof(Catalogos), new { tipo, pagina });
    }

    [HttpGet]
    public async Task<IActionResult> DetalleUsuario(int id)
    {
        var usuario = await _db.BuscarDetalleEmpleadoAsync(id);

        return usuario is null ? NotFound() : View(usuario);
    }

    [HttpGet]
    public IActionResult Registro() => RedirectToAction(nameof(Empleados));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistroUsuarioViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ApellidoPaterno) && string.IsNullOrWhiteSpace(model.ApellidoMaterno))
            ModelState.AddModelError(nameof(model.ApellidoPaterno), "Debes proporcionar al menos un apellido.");

        if (await _db.Usuarios.Query.AnyAsync(u => u.CorreoEmpresarial == model.CorreoEmpresarial))
            ModelState.AddModelError(nameof(model.CorreoEmpresarial), "El correo empresarial ya está registrado.");
        if (await _db.LoginExistsAsync(model.UsuarioLogin))
            ModelState.AddModelError(nameof(model.UsuarioLogin), "El nombre de usuario ya existe.");

        if (!ModelState.IsValid)
            return View("Registro", await BuildRegistroViewModelAsync(model));

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
        TempData["Mensaje"] = $"Usuario registrado correctamente (ID {usuario.Id}).";
        return RedirectToAction(nameof(Empleados));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarUsuarioEstatus(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        usuario.Estatus = "INACTIVO";
        await _db.CambiarEstatusEmpleadoAsync(usuario);
        TempData["Mensaje"] = "Empleado dado de baja.";
        return RedirectToAction(nameof(Empleados), new { numero = id });
    }

    [HttpGet]
    public async Task<IActionResult> AsignarCurso(int id)
    {
        var usuario = await _db.Usuarios.Query.FirstOrDefaultAsync(u => u.Id == id);
        if (usuario is null) return NotFound();

        return View(await BuildAsignarCursoViewModelAsync(usuario.Id, usuario.Nombres, usuario.CorreoEmpresarial));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarCurso(AsignarCursoViewModel model)
    {
        var usuario = await _db.Usuarios.FindAsync(model.UsuarioId);
        if (usuario is null) return NotFound();

        if (!ModelState.IsValid)
            return View(await BuildAsignarCursoViewModelAsync(usuario.Id, usuario.Nombres, usuario.CorreoEmpresarial, model.CursoId));
        var curso = await _db.Cursos.FindAsync(model.CursoId);
        if (curso?.Estatus != "activo")
        {
            ModelState.AddModelError(nameof(model.CursoId), "El curso seleccionado no existe o está inactivo.");
            return View(await BuildAsignarCursoViewModelAsync(usuario.Id, usuario.Nombres, usuario.CorreoEmpresarial, model.CursoId));
        }
        if (await _db.TieneCursoAsync(model.UsuarioId, model.CursoId))
        {
            ModelState.AddModelError(nameof(model.CursoId), "El usuario ya está inscrito en este curso.");
            return View(await BuildAsignarCursoViewModelAsync(usuario.Id, usuario.Nombres, usuario.CorreoEmpresarial, model.CursoId));
        }

        if (!await _db.AsignarCursoAsync(model.UsuarioId, new CursoUsuario
        {
            CursoId = model.CursoId,
            NombreCurso = curso.Nombre,
            DuracionMinutos = curso.DuracionMinutos,
            FechaInicio = DateTime.UtcNow,
            Estatus = "EN CURSO"
        }))
            return Conflict("La inscripción ya existe o el empleado fue eliminado.");
        TempData["Mensaje"] = "Curso asignado correctamente.";
        return RedirectToAction(nameof(DetalleUsuario), new { id = model.UsuarioId });
    }

    private async Task<CategoriasViewModel> BuildCategoriasViewModelAsync(CategoriaCreateViewModel? nueva = null)
    {
        return new CategoriasViewModel
        {
            NuevaCategoria = nueva ?? new CategoriaCreateViewModel(),
            Categorias = await _db.Categorias.Query.OrderBy(c => c.Nombre).ToListAsync()
        };
    }

    private async Task<CursoCreateViewModel> BuildCursoViewModelAsync(CursoCreateViewModel model)
    {
        model.Categorias = await _db.Categorias.Query.Where(c => c.Estatus == "activo").OrderBy(c => c.Nombre).ToListAsync();
        model.Niveles = await _db.Niveles.Query.Where(n => n.Estatus == "activo").OrderBy(n => n.Nombre).ToListAsync();
        model.Instructores = await _db.Instructores.Query.Where(i => i.Estatus == "activo").OrderBy(i => i.Nombre).ToListAsync();
        return model;
    }

    private async Task<RegistroUsuarioViewModel> BuildRegistroViewModelAsync(RegistroUsuarioViewModel model)
    {
        model.Estados = await _db.Estados.Query.Where(e => e.Estatus == "ACTIVO").OrderBy(e => e.NombreEstado).ToListAsync();
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
            Cursos = await _db.Cursos.Query
                .Where(c => c.Estatus == "activo")
                .OrderBy(c => c.Nombre)
                .ToListAsync()
        };
    }

    private async Task<EmpleadosViewModel> BuildEmpleadosViewModelAsync(string? q, string? estatus, int pagina, int? numero)
    {
        var model = new EmpleadosViewModel { Q = q, Numero = numero, Estatus = estatus };
        if (string.IsNullOrWhiteSpace(q) && estatus is null && numero is null) return model;

        model.BusquedaAplicada = true;
        const int tamanoPagina = 8;
        var query = _db.FiltrarEmpleados(q, numero, estatus);
        model.TotalEmpleados = await query.CountAsync();
        model.TotalPaginas = Math.Max(1, (int)Math.Ceiling((double)model.TotalEmpleados / tamanoPagina));
        model.Pagina = Math.Clamp(pagina, 1, model.TotalPaginas);
        model.Empleados = await query.OrderBy(u => u.Id).Skip((model.Pagina - 1) * tamanoPagina)
            .Take(tamanoPagina).ToListAsync();
        return model;
    }

    private async Task<EmpleadoFormViewModel> BuildEmpleadoFormViewModelAsync(EmpleadoFormViewModel? model = null)
    {
        model ??= new EmpleadoFormViewModel();
        model.Estados = await _db.Estados.Query.Where(e => e.Estatus == "ACTIVO" || e.Id == model.EstadoId)
            .OrderBy(e => e.NombreEstado).ToListAsync();
        model.Sexos = (await _db.Sexos.Query.OrderBy(s => s.Descripcion).ToListAsync())
            .Where(s => s.Estatus == "ACTIVO" || s.Id == model.SexoId).ToList();
        return model;
    }

    private async Task<CatalogosViewModel> BuildCatalogosViewModelAsync(
        string tipo,
        string? editar = null,
        CatalogoElementoViewModel? elemento = null,
        bool mostrarFormulario = false,
        int pagina = 1)
    {
        const int tamanoPagina = 8;
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
        model.TotalPaginas = Math.Max(1, (int)Math.Ceiling(model.TotalElementos / (double)tamanoPagina));
        model.Pagina = Math.Clamp(pagina, 1, model.TotalPaginas);
        switch (tipo)
        {
            case "sexos":
                var sexos = await _db.Sexos.Query.OrderBy(s => s.Descripcion)
                    .Skip((model.Pagina - 1) * tamanoPagina).Take(tamanoPagina).ToListAsync();
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
                    .Skip((model.Pagina - 1) * tamanoPagina).Take(tamanoPagina)
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

    private static bool EsTipoCatalogoValido(string tipo) =>
        tipo is "sexos" or "estados";
}
