using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoSqlU.Api.Models.ViewModels;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Controllers.Mvc;

public class AdminController : Controller
{
    private readonly NoSqlUContext _db;

    public AdminController(NoSqlUContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            TotalCursos = await _db.Cursos.CountAsync(),
            TotalCategorias = await _db.Categorias.CountAsync(),
            CursosActivos = await _db.Cursos.CountAsync(c => c.Estatus == "activo"),
            Cursos = await _db.Cursos
                .AsNoTracking()
                .Include(c => c.Categoria)
                .Include(c => c.Nivel)
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync()
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
        if (await _db.Categorias.AnyAsync(c => c.Nombre == nombre))
        {
            ModelState.AddModelError(nameof(nuevaCategoria.Nombre), "Ya existe una categoría con ese nombre.");
            return View("Categorias", await BuildCategoriasViewModelAsync(nuevaCategoria));
        }

        _db.Categorias.Add(new Categoria
        {
            Nombre = nombre,
            Descripcion = nuevaCategoria.Descripcion?.Trim(),
            Estatus = "activo",
            FechaCreacion = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
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
        await _db.SaveChangesAsync();
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

        var categoria = await _db.Categorias
            .FirstOrDefaultAsync(c => c.Id == model.CategoriaId && c.Estatus == "activo");
        if (categoria is null)
        {
            ModelState.AddModelError(nameof(model.CategoriaId), "La categoría seleccionada no existe o está inactiva.");
            return View("Cursos", await BuildCursoViewModelAsync(model));
        }

        var nombreNivel = model.Nivel.Trim();
        var nivel = await _db.Niveles.FirstOrDefaultAsync(n => n.Nombre == nombreNivel);
        if (nivel is null)
        {
            nivel = new Nivel { Nombre = nombreNivel, Estatus = "activo", FechaCreacion = DateTime.UtcNow };
            _db.Niveles.Add(nivel);
        }
        else
        {
            nivel.Estatus = "activo";
        }

        var nombreInstructor = model.Instructor.Trim();
        var instructor = await _db.Instructores.FirstOrDefaultAsync(i => i.Nombre == nombreInstructor);
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
            _db.Instructores.Add(instructor);
        }
        else
        {
            instructor.Estatus = "activo";
        }

        _db.Cursos.Add(new Curso
        {
            Nombre = model.Nombre.Trim(),
            Descripcion = model.Descripcion.Trim(),
            Precio = model.Precio,
            DuracionMinutos = model.DuracionMinutos,
            FechaPublicacion = model.FechaPublicacion!.Value,
            Categoria = categoria,
            Nivel = nivel,
            Instructor = instructor,
            Estatus = "activo",
            FechaCreacion = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
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
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Buscar(BuscarCursosViewModel model)
    {
        var query = _db.Cursos
            .AsNoTracking()
            .Include(c => c.Categoria)
            .Include(c => c.Nivel)
            .Include(c => c.Instructor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(model.Q))
            query = query.Where(c => c.Nombre.Contains(model.Q) || c.Descripcion.Contains(model.Q));
        if (!string.IsNullOrWhiteSpace(model.Categoria))
            query = query.Where(c => c.Categoria!.Nombre == model.Categoria);
        if (!string.IsNullOrWhiteSpace(model.Nivel))
            query = query.Where(c => c.Nivel!.Nombre == model.Nivel);
        if (!string.IsNullOrWhiteSpace(model.Instructor))
            query = query.Where(c => c.Instructor!.Nombre == model.Instructor);
        if (!string.IsNullOrWhiteSpace(model.Estatus))
            query = query.Where(c => c.Estatus == model.Estatus);
        if (model.Min.HasValue)
            query = query.Where(c => c.Precio >= model.Min.Value);
        if (model.Max.HasValue)
            query = query.Where(c => c.Precio <= model.Max.Value);

        model.Categorias = await _db.Categorias.AsNoTracking().Where(c => c.Estatus == "activo").OrderBy(c => c.Nombre).ToListAsync();
        model.Niveles = await _db.Niveles.AsNoTracking().Where(n => n.Estatus == "activo").OrderBy(n => n.Nombre).ToListAsync();
        model.Instructores = await _db.Instructores.AsNoTracking().Where(i => i.Estatus == "activo").OrderBy(i => i.Nombre).ToListAsync();
        model.Resultados = await query.OrderBy(c => c.Nombre).ToListAsync();
        return View(model);
    }

    [HttpGet]
    public IActionResult Usuarios() => RedirectToAction(nameof(Empleados));

    [HttpGet]
    public async Task<IActionResult> Empleados(string? q, string? estatus, int pagina = 1)
    {
        if (!string.IsNullOrWhiteSpace(estatus) && estatus != "ACTIVO" && estatus != "INACTIVO")
            return BadRequest("El filtro de estatus debe ser ACTIVO o INACTIVO.");
        return View(await BuildEmpleadosViewModelAsync(q, estatus, pagina));
    }

    [HttpGet]
    public async Task<IActionResult> BuscarEmpleados(string? q, string? estatus, int pagina = 1)
    {
        if (!string.IsNullOrWhiteSpace(estatus) && estatus != "ACTIVO" && estatus != "INACTIVO")
            return BadRequest("El filtro de estatus debe ser ACTIVO o INACTIVO.");

        var model = await BuildEmpleadosViewModelAsync(q, estatus, pagina);
        return Json(new
        {
            employees = model.Empleados.Select(u => new
            {
                id = u.Id,
                name = $"{u.Nombres} {u.ApellidoPaterno} {u.ApellidoMaterno}".Trim(),
                email = u.CorreoEmpresarial,
                status = u.Estatus,
                joinDate = u.FechaRegistro.ToString("d")
            }),
            model.Pagina,
            model.TotalPaginas,
            model.TotalEmpleados
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarEmpleado(
        [Bind(Prefix = "NuevoEmpleado")] EmpleadoFormViewModel model,
        string? q,
        string? estatusActual,
        int pagina = 1)
    {
        if (model.Cursos.Count == 0)
            ModelState.AddModelError("NuevoEmpleado.Cursos", "Registra al menos un curso del certificado.");

        if (await _db.Usuarios.AnyAsync(u => u.CorreoEmpresarial == model.CorreoEmpresarial.Trim()))
            ModelState.AddModelError("NuevoEmpleado.CorreoEmpresarial", "El correo empresarial ya está registrado.");

        var sexoValido = model.SexoId.HasValue &&
            await _db.Sexos.AnyAsync(s => s.Id == model.SexoId.Value && s.Estatus == "ACTIVO");
        if (!sexoValido)
            ModelState.AddModelError("NuevoEmpleado.SexoId", "Selecciona un sexo activo.");

        if (!await _db.Estados.AnyAsync(e => e.Id == model.EstadoId && e.Estatus == "ACTIVO"))
            ModelState.AddModelError("NuevoEmpleado.EstadoId", "Selecciona un estado activo.");

        if (model.Cursos.Any(c => c.Estatus != "COMPLETADO" && c.Estatus != "EN CURSO"))
            ModelState.AddModelError("NuevoEmpleado.Cursos", "El estatus de cada curso debe ser válido.");
        for (var i = 0; i < model.Cursos.Count; i++)
        {
            if (model.Cursos[i].Estatus == "COMPLETADO" && !model.Cursos[i].FechaFinalizacion.HasValue)
                ModelState.AddModelError($"NuevoEmpleado.Cursos[{i}].FechaFinalizacion", "Indica la fecha de finalización del curso.");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await BuildEmpleadosViewModelAsync(q, estatusActual, pagina, model, mostrarFormulario: true);
            return View("Empleados", viewModel);
        }

        var usuario = new Usuario
        {
            Nombres = model.Nombres.Trim(),
            ApellidoPaterno = model.ApellidoPaterno.Trim(),
            ApellidoMaterno = model.ApellidoMaterno?.Trim(),
            FechaNacimiento = model.FechaNacimiento!.Value,
            SexoId = model.SexoId!.Value,
            CorreoEmpresarial = model.CorreoEmpresarial.Trim(),
            Telefono = model.Telefono?.Trim(),
            FechaRegistro = model.FechaIngreso!.Value,
            Estatus = "ACTIVO",
            Direcciones = new List<DireccionUsuario>
            {
                new()
                {
                    EstadoId = model.EstadoId,
                    Calle = model.Calle.Trim(),
                    NumeroExterior = model.NumeroExterior.Trim(),
                    NumeroInterior = model.NumeroInterior?.Trim(),
                    CodigoPostal = model.CodigoPostal.Trim(),
                    Colonia = model.Colonia.Trim(),
                    Municipio = model.Municipio.Trim()
                }
            },
            CursosUsuarios = model.Cursos.Select(c => new CursoUsuario
            {
                NombreCurso = c.NombreCurso.Trim(),
                Institucion = c.Institucion.Trim(),
                FechaFinalizacion = c.FechaFinalizacion,
                Estatus = c.Estatus
            }).ToList()
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        TempData["Mensaje"] = $"Empleado registrado correctamente (No. {usuario.Id}).";
        return RedirectToAction(nameof(Empleados));
    }

    [HttpGet]
    public async Task<IActionResult> EditarEmpleado(int id)
    {
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Direcciones)
            .Include(u => u.CursosUsuarios)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (usuario is null) return NotFound();

        var direccion = usuario.Direcciones.FirstOrDefault();
        var model = new EmpleadoFormViewModel
        {
            Id = usuario.Id,
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
            Colonia = direccion?.Colonia ?? string.Empty,
            Municipio = direccion?.Municipio ?? string.Empty,
            Cursos = usuario.CursosUsuarios.Select(c => new CursoEmpleadoFormViewModel
            {
                NombreCurso = c.NombreCurso,
                Institucion = c.Institucion ?? string.Empty,
                FechaFinalizacion = c.FechaFinalizacion,
                Estatus = c.Estatus
            }).ToList(),
            Estados = await _db.Estados.AsNoTracking().Where(e => e.Estatus == "ACTIVO").OrderBy(e => e.NombreEstado).ToListAsync(),
            Sexos = await _db.Sexos.AsNoTracking().Where(s => s.Estatus == "ACTIVO" || s.Id == usuario.SexoId).OrderBy(s => s.Descripcion).ToListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarEmpleado(EmpleadoFormViewModel model)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Direcciones)
            .Include(u => u.CursosUsuarios)
            .FirstOrDefaultAsync(u => u.Id == model.Id);
        if (usuario is null) return NotFound();

        if (model.Cursos.Count == 0)
            ModelState.AddModelError(nameof(model.Cursos), "Registra al menos un curso del certificado.");
        if (model.Cursos.Any(c => c.Estatus != "COMPLETADO" && c.Estatus != "EN CURSO"))
            ModelState.AddModelError(nameof(model.Cursos), "El estatus de cada curso debe ser válido.");
        for (var i = 0; i < model.Cursos.Count; i++)
        {
            if (model.Cursos[i].Estatus == "COMPLETADO" && !model.Cursos[i].FechaFinalizacion.HasValue)
                ModelState.AddModelError($"Cursos[{i}].FechaFinalizacion", "Indica la fecha de finalización del curso.");
        }

        if (await _db.Usuarios.AnyAsync(u => u.Id != model.Id && u.CorreoEmpresarial == model.CorreoEmpresarial.Trim()))
            ModelState.AddModelError(nameof(model.CorreoEmpresarial), "El correo empresarial ya está registrado.");
        if (!model.SexoId.HasValue || !await _db.Sexos.AnyAsync(s => s.Id == model.SexoId.Value && s.Estatus == "ACTIVO"))
            ModelState.AddModelError(nameof(model.SexoId), "Selecciona un sexo activo.");
        if (!await _db.Estados.AnyAsync(e => e.Id == model.EstadoId && e.Estatus == "ACTIVO"))
            ModelState.AddModelError(nameof(model.EstadoId), "Selecciona un estado activo.");

        if (!ModelState.IsValid)
        {
            model.Estados = await _db.Estados.AsNoTracking().Where(e => e.Estatus == "ACTIVO").OrderBy(e => e.NombreEstado).ToListAsync();
            model.Sexos = await _db.Sexos.AsNoTracking().Where(s => s.Estatus == "ACTIVO").OrderBy(s => s.Descripcion).ToListAsync();
            return View(model);
        }

        usuario.Nombres = model.Nombres.Trim();
        usuario.ApellidoPaterno = model.ApellidoPaterno.Trim();
        usuario.ApellidoMaterno = model.ApellidoMaterno?.Trim();
        usuario.FechaNacimiento = model.FechaNacimiento!.Value;
        usuario.FechaRegistro = model.FechaIngreso!.Value;
        usuario.SexoId = model.SexoId.GetValueOrDefault();
        usuario.CorreoEmpresarial = model.CorreoEmpresarial.Trim();
        usuario.Telefono = model.Telefono?.Trim();

        var direccion = usuario.Direcciones.FirstOrDefault();
        if (direccion is null)
        {
            direccion = new DireccionUsuario { UsuarioId = usuario.Id };
            _db.DireccionesUsuarios.Add(direccion);
        }
        direccion.EstadoId = model.EstadoId;
        direccion.Calle = model.Calle.Trim();
        direccion.NumeroExterior = model.NumeroExterior.Trim();
        direccion.NumeroInterior = model.NumeroInterior?.Trim();
        direccion.CodigoPostal = model.CodigoPostal.Trim();
        direccion.Colonia = model.Colonia.Trim();
        direccion.Municipio = model.Municipio.Trim();

        _db.CursosUsuarios.RemoveRange(usuario.CursosUsuarios);
        usuario.CursosUsuarios = model.Cursos.Select(c => new CursoUsuario
        {
            NombreCurso = c.NombreCurso.Trim(),
            Institucion = c.Institucion.Trim(),
            FechaFinalizacion = c.FechaFinalizacion,
            Estatus = c.Estatus
        }).ToList();

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Datos del empleado actualizados correctamente.";
        return RedirectToAction(nameof(Empleados));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarEmpleado(int id, string? q, string? filtroEstatus, int pagina = 1)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.CursosUsuarios)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (usuario is null) return NotFound();

        _db.CursosUsuarios.RemoveRange(usuario.CursosUsuarios);
        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "El empleado y sus datos relacionados se eliminaron permanentemente.";
        return RedirectToAction(nameof(Empleados), new { q, estatus = filtroEstatus, pagina });
    }

    [HttpGet]
    public async Task<IActionResult> Catalogos(string tipo = "categorias", string? editar = null, int pagina = 1)
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
        if (tipo == "instructores" && elemento.Descripcion?.Length > 150)
            ModelState.AddModelError("Elemento.Descripcion", "El correo no puede superar 150 caracteres.");

        if (ModelState.IsValid)
        {
            var duplicado = await ExisteCatalogoDuplicadoAsync(tipo, elemento.Nombre.Trim(), elemento.Clave?.Trim(), claveOriginal);
            if (duplicado)
                ModelState.AddModelError("Elemento.Nombre", "Ya existe un elemento con ese nombre o clave.");
        }

        if (!ModelState.IsValid)
            return View("Catalogos", await BuildCatalogosViewModelAsync(tipo, claveOriginal, elemento, mostrarFormulario: true, pagina: pagina));

        var editar = !string.IsNullOrWhiteSpace(claveOriginal);
        var estatus = elemento.Estatus == "inactivo" ? "inactivo" : "activo";
        switch (tipo)
        {
            case "categorias":
                var categoria = editar
                    ? await _db.Categorias.FirstOrDefaultAsync(c => c.Id == int.Parse(claveOriginal!))
                    : new Categoria { FechaCreacion = DateTime.UtcNow };
                if (categoria is null) return NotFound();
                categoria.Nombre = elemento.Nombre.Trim();
                categoria.Descripcion = elemento.Descripcion?.Trim();
                categoria.Estatus = estatus;
                if (!editar) _db.Categorias.Add(categoria);
                break;
            case "niveles":
                var nivel = editar
                    ? await _db.Niveles.FirstOrDefaultAsync(n => n.Id == int.Parse(claveOriginal!))
                    : new Nivel { FechaCreacion = DateTime.UtcNow };
                if (nivel is null) return NotFound();
                nivel.Nombre = elemento.Nombre.Trim();
                nivel.Estatus = estatus;
                if (!editar) _db.Niveles.Add(nivel);
                break;
            case "instructores":
                var instructor = editar
                    ? await _db.Instructores.FirstOrDefaultAsync(i => i.Id == int.Parse(claveOriginal!))
                    : new Instructor { FechaCreacion = DateTime.UtcNow, Apellido = string.Empty };
                if (instructor is null) return NotFound();
                instructor.Nombre = elemento.Nombre.Trim();
                instructor.Email = elemento.Descripcion?.Trim();
                instructor.Estatus = estatus;
                if (!editar) _db.Instructores.Add(instructor);
                break;
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
                sexo.Estatus = estatus.ToUpperInvariant();
                if (!editar) _db.Sexos.Add(sexo);
                break;
            case "estados":
                var estado = editar
                    ? await _db.Estados.FirstOrDefaultAsync(e => e.Id == int.Parse(claveOriginal!))
                    : new Estado();
                if (estado is null) return NotFound();
                estado.NombreEstado = elemento.Nombre.Trim();
                estado.Estatus = estatus.ToUpperInvariant();
                if (!editar) _db.Estados.Add(estado);
                break;
        }

        await _db.SaveChangesAsync();
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
            case "categorias":
                var categoria = await _db.Categorias.FindAsync(int.Parse(clave));
                if (categoria is null) return NotFound();
                categoria.Estatus = categoria.Estatus == "activo" ? "inactivo" : "activo";
                break;
            case "niveles":
                var nivel = await _db.Niveles.FindAsync(int.Parse(clave));
                if (nivel is null) return NotFound();
                nivel.Estatus = nivel.Estatus == "activo" ? "inactivo" : "activo";
                break;
            case "instructores":
                var instructor = await _db.Instructores.FindAsync(int.Parse(clave));
                if (instructor is null) return NotFound();
                instructor.Estatus = instructor.Estatus == "activo" ? "inactivo" : "activo";
                break;
            case "sexos":
                var sexo = await _db.Sexos.FindAsync(char.Parse(clave));
                if (sexo is null) return NotFound();
                sexo.Estatus = sexo.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
                break;
            case "estados":
                var estado = await _db.Estados.FindAsync(int.Parse(clave));
                if (estado is null) return NotFound();
                estado.Estatus = estado.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
                break;
        }
        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Estatus del elemento actualizado; se conserva el historial asociado.";
        return RedirectToAction(nameof(Catalogos), new { tipo, pagina });
    }

    [HttpGet]
    public async Task<IActionResult> DetalleUsuario(int id)
    {
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.Sexo)
            .Include(u => u.Contrasena)
            .Include(u => u.Direcciones).ThenInclude(d => d.Estado)
            .Include(u => u.CursosUsuarios).ThenInclude(cu => cu.Curso)
            .FirstOrDefaultAsync(u => u.Id == id);

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

        if (await _db.Usuarios.AnyAsync(u => u.CorreoEmpresarial == model.CorreoEmpresarial))
            ModelState.AddModelError(nameof(model.CorreoEmpresarial), "El correo empresarial ya está registrado.");
        if (await _db.UsuariosContrasenas.AnyAsync(u => u.UsuarioLogin == model.UsuarioLogin))
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

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        TempData["Mensaje"] = $"Usuario registrado correctamente (ID {usuario.Id}).";
        return RedirectToAction(nameof(Empleados));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarUsuarioEstatus(int id, string? q, string? filtroEstatus, int pagina = 1)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        usuario.Estatus = usuario.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Empleados), new { q, estatus = filtroEstatus, pagina });
    }

    [HttpGet]
    public async Task<IActionResult> AsignarCurso(int id)
    {
        var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
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
        if (!await _db.Cursos.AnyAsync(c => c.Id == model.CursoId && c.Estatus == "activo"))
        {
            ModelState.AddModelError(nameof(model.CursoId), "El curso seleccionado no existe o está inactivo.");
            return View(await BuildAsignarCursoViewModelAsync(usuario.Id, usuario.Nombres, usuario.CorreoEmpresarial, model.CursoId));
        }
        if (await _db.CursosUsuarios.AnyAsync(cu => cu.UsuarioId == model.UsuarioId && cu.CursoId == model.CursoId))
        {
            ModelState.AddModelError(nameof(model.CursoId), "El usuario ya está inscrito en este curso.");
            return View(await BuildAsignarCursoViewModelAsync(usuario.Id, usuario.Nombres, usuario.CorreoEmpresarial, model.CursoId));
        }

        _db.CursosUsuarios.Add(new CursoUsuario
        {
            UsuarioId = model.UsuarioId,
            CursoId = model.CursoId,
            NombreCurso = await _db.Cursos.Where(c => c.Id == model.CursoId).Select(c => c.Nombre).FirstAsync(),
            FechaInicio = DateTime.UtcNow,
            Estatus = "EN CURSO"
        });
        await _db.SaveChangesAsync();
        TempData["Mensaje"] = "Curso asignado correctamente.";
        return RedirectToAction(nameof(DetalleUsuario), new { id = model.UsuarioId });
    }

    private async Task<CategoriasViewModel> BuildCategoriasViewModelAsync(CategoriaCreateViewModel? nueva = null)
    {
        return new CategoriasViewModel
        {
            NuevaCategoria = nueva ?? new CategoriaCreateViewModel(),
            Categorias = await _db.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync()
        };
    }

    private async Task<CursoCreateViewModel> BuildCursoViewModelAsync(CursoCreateViewModel model)
    {
        model.Categorias = await _db.Categorias.AsNoTracking().Where(c => c.Estatus == "activo").OrderBy(c => c.Nombre).ToListAsync();
        model.Niveles = await _db.Niveles.AsNoTracking().Where(n => n.Estatus == "activo").OrderBy(n => n.Nombre).ToListAsync();
        model.Instructores = await _db.Instructores.AsNoTracking().Where(i => i.Estatus == "activo").OrderBy(i => i.Nombre).ToListAsync();
        return model;
    }

    private async Task<RegistroUsuarioViewModel> BuildRegistroViewModelAsync(RegistroUsuarioViewModel model)
    {
        model.Estados = await _db.Estados.AsNoTracking().Where(e => e.Estatus == "ACTIVO").OrderBy(e => e.NombreEstado).ToListAsync();
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
            Cursos = await _db.Cursos.AsNoTracking()
                .Where(c => c.Estatus == "activo")
                .OrderBy(c => c.Nombre)
                .ToListAsync()
        };
    }

    private async Task<EmpleadosViewModel> BuildEmpleadosViewModelAsync(
        string? q,
        string? estatus,
        int pagina,
        EmpleadoFormViewModel? nuevo = null,
        bool mostrarFormulario = false)
    {
        const int tamanoPagina = 8;
        var query = _db.Usuarios.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var busqueda = q.Trim();
            var numeroEmpleado = int.TryParse(busqueda, out var idBuscado) ? idBuscado : (int?)null;
            query = query.Where(u =>
                u.Nombres.Contains(busqueda) ||
                (u.ApellidoPaterno != null && u.ApellidoPaterno.Contains(busqueda)) ||
                (u.ApellidoMaterno != null && u.ApellidoMaterno.Contains(busqueda)) ||
                u.CorreoEmpresarial.Contains(busqueda) ||
                (numeroEmpleado.HasValue && u.Id == numeroEmpleado.Value));
        }
        if (estatus == "ACTIVO" || estatus == "INACTIVO")
            query = query.Where(u => u.Estatus == estatus);

        var totalEmpleados = await query.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalEmpleados / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);
        var empleados = await query
            .OrderBy(u => u.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return new EmpleadosViewModel
        {
            Q = q,
            Estatus = estatus,
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            TotalEmpleados = totalEmpleados,
            Empleados = empleados,
            NuevoEmpleado = await BuildEmpleadoFormViewModelAsync(nuevo),
            MostrarFormulario = mostrarFormulario
        };
    }

    private async Task<EmpleadoFormViewModel> BuildEmpleadoFormViewModelAsync(EmpleadoFormViewModel? model = null)
    {
        model ??= new EmpleadoFormViewModel();
        if (model.Cursos.Count == 0)
            model.Cursos.Add(new CursoEmpleadoFormViewModel());
        model.Estados = await _db.Estados.AsNoTracking()
            .Where(e => e.Estatus == "ACTIVO")
            .OrderBy(e => e.NombreEstado)
            .ToListAsync();
        model.Sexos = await _db.Sexos.AsNoTracking()
            .Where(s => s.Estatus == "ACTIVO")
            .OrderBy(s => s.Descripcion)
            .ToListAsync();
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
            case "categorias":
                model.TotalElementos = await _db.Categorias.CountAsync();
                break;
            case "niveles":
                model.TotalElementos = await _db.Niveles.CountAsync();
                break;
            case "instructores":
                model.TotalElementos = await _db.Instructores.CountAsync();
                break;
            case "sexos":
                model.TotalElementos = await _db.Sexos.CountAsync();
                break;
            case "estados":
                model.TotalElementos = await _db.Estados.CountAsync();
                break;
        }
        model.TotalPaginas = Math.Max(1, (int)Math.Ceiling(model.TotalElementos / (double)tamanoPagina));
        model.Pagina = Math.Clamp(pagina, 1, model.TotalPaginas);
        switch (tipo)
        {
            case "categorias":
                model.Elementos = await _db.Categorias.AsNoTracking().OrderBy(c => c.Nombre)
                    .Skip((model.Pagina - 1) * tamanoPagina).Take(tamanoPagina)
                    .Select(c => new CatalogoFilaViewModel { Clave = c.Id.ToString(), Nombre = c.Nombre, Descripcion = c.Descripcion, Estatus = c.Estatus }).ToListAsync();
                if (elemento is null && int.TryParse(editar, out var idCategoria))
                {
                    var item = await _db.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == idCategoria);
                    if (item is null) return model;
                    elemento = new CatalogoElementoViewModel { Nombre = item.Nombre, Descripcion = item.Descripcion, Estatus = item.Estatus };
                }
                break;
            case "niveles":
                model.Elementos = await _db.Niveles.AsNoTracking().OrderBy(n => n.Nombre)
                    .Skip((model.Pagina - 1) * tamanoPagina).Take(tamanoPagina)
                    .Select(n => new CatalogoFilaViewModel { Clave = n.Id.ToString(), Nombre = n.Nombre, Estatus = n.Estatus }).ToListAsync();
                if (elemento is null && int.TryParse(editar, out var idNivel))
                {
                    var item = await _db.Niveles.AsNoTracking().FirstOrDefaultAsync(n => n.Id == idNivel);
                    if (item is null) return model;
                    elemento = new CatalogoElementoViewModel { Nombre = item.Nombre, Estatus = item.Estatus };
                }
                break;
            case "instructores":
                model.Elementos = await _db.Instructores.AsNoTracking().OrderBy(i => i.Nombre)
                    .Skip((model.Pagina - 1) * tamanoPagina).Take(tamanoPagina)
                    .Select(i => new CatalogoFilaViewModel { Clave = i.Id.ToString(), Nombre = i.Nombre, Descripcion = i.Email, Estatus = i.Estatus }).ToListAsync();
                if (elemento is null && int.TryParse(editar, out var idInstructor))
                {
                    var item = await _db.Instructores.AsNoTracking().FirstOrDefaultAsync(i => i.Id == idInstructor);
                    if (item is null) return model;
                    elemento = new CatalogoElementoViewModel { Nombre = item.Nombre, Descripcion = item.Email, Estatus = item.Estatus };
                }
                break;
            case "sexos":
                model.Elementos = await _db.Sexos.AsNoTracking().OrderBy(s => s.Descripcion)
                    .Skip((model.Pagina - 1) * tamanoPagina).Take(tamanoPagina)
                    .Select(s => new CatalogoFilaViewModel { Clave = s.Id.ToString(), Nombre = s.Descripcion, Estatus = s.Estatus }).ToListAsync();
                if (elemento is null && !string.IsNullOrWhiteSpace(editar) && char.TryParse(editar, out var claveSexo))
                {
                    var item = await _db.Sexos.AsNoTracking().FirstOrDefaultAsync(s => s.Id == claveSexo);
                    if (item is null) return model;
                    elemento = new CatalogoElementoViewModel { Clave = item.Id.ToString(), Nombre = item.Descripcion, Estatus = item.Estatus.ToLowerInvariant() };
                }
                break;
            case "estados":
                model.Elementos = await _db.Estados.AsNoTracking().OrderBy(e => e.NombreEstado)
                    .Skip((model.Pagina - 1) * tamanoPagina).Take(tamanoPagina)
                    .Select(e => new CatalogoFilaViewModel { Clave = e.Id.ToString(), Nombre = e.NombreEstado, Estatus = e.Estatus }).ToListAsync();
                if (elemento is null && int.TryParse(editar, out var idEstado))
                {
                    var item = await _db.Estados.AsNoTracking().FirstOrDefaultAsync(e => e.Id == idEstado);
                    if (item is null) return model;
                    elemento = new CatalogoElementoViewModel { Nombre = item.NombreEstado, Estatus = item.Estatus.ToLowerInvariant() };
                }
                break;
        }
        model.Elemento = elemento ?? new CatalogoElementoViewModel();
        return model;
    }

    private async Task<bool> ExisteCatalogoDuplicadoAsync(string tipo, string nombre, string? clave, string? claveOriginal)
    {
        var originalId = int.TryParse(claveOriginal, out var parsedId) ? parsedId : (int?)null;
        var originalSex = char.TryParse(claveOriginal, out var parsedSex) ? parsedSex : (char?)null;
        switch (tipo)
        {
            case "categorias":
                var categoriaDuplicada = await _db.Categorias.AnyAsync(c => c.Nombre == nombre && (!originalId.HasValue || c.Id != originalId.Value));
                return categoriaDuplicada;
            case "niveles":
                var nivelDuplicado = await _db.Niveles.AnyAsync(n => n.Nombre == nombre && (!originalId.HasValue || n.Id != originalId.Value));
                return nivelDuplicado;
            case "instructores":
                var instructorDuplicado = await _db.Instructores.AnyAsync(i => i.Nombre == nombre && (!originalId.HasValue || i.Id != originalId.Value));
                return instructorDuplicado;
            case "sexos":
                if (string.IsNullOrWhiteSpace(clave)) return false;
                var sexoDuplicado = await _db.Sexos.AnyAsync(s =>
                    (s.Descripcion == nombre && (!originalSex.HasValue || s.Id != originalSex.Value)) ||
                    (s.Id == clave.Trim().ToUpperInvariant()[0] && (!originalSex.HasValue || s.Id != originalSex.Value)));
                return sexoDuplicado;
            case "estados":
                return await _db.Estados.AnyAsync(e => e.NombreEstado == nombre && (!originalId.HasValue || e.Id != originalId.Value));
            default:
                return false;
        }
    }

    private static bool EsTipoCatalogoValido(string tipo) =>
        tipo is "categorias" or "niveles" or "instructores" or "sexos" or "estados";
}
