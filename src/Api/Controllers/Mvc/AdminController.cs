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
    public async Task<IActionResult> Usuarios()
    {
        var usuarios = await _db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToListAsync();
        return View(usuarios);
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
    public async Task<IActionResult> Registro()
    {
        return View(await BuildRegistroViewModelAsync(new RegistroUsuarioViewModel()));
    }

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
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarUsuarioEstatus(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        usuario.Estatus = usuario.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Usuarios));
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
        model.Estados = await _db.Estados.AsNoTracking().OrderBy(e => e.NombreEstado).ToListAsync();
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
}
