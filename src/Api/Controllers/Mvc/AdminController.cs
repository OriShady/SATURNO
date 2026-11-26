using Microsoft.AspNetCore.Mvc;
using NoSqlU.Api.Models;
using NoSqlU.Api.Models.ViewModels;

namespace NoSqlU.Api.Controllers.Mvc;

public class AdminController : Controller
{
    private readonly AdminModel _model;

    public AdminController(AdminModel model) => _model = model;

    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1)
    {
        return View(await _model.IndexAsync(pagina));
    }

    [HttpGet]
    public async Task<IActionResult> Categorias(int pagina = 1)
    {
        return View(await _model.CategoriasAsync(pagina));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(CategoriaCreateViewModel nuevaCategoria, int pagina = 1)
    {
        var resultado = await _model.CrearCategoriaAsync(nuevaCategoria, ModelState.IsValid, pagina);
        if (resultado.Estado == AdminEstado.FormularioInvalido)
        {
            return FormularioInvalido(resultado, "Categorias");
        }
        TempData["Mensaje"] = "Categoría creada correctamente.";
        return RedirectToAction(nameof(Categorias), new { pagina });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarCategoriaEstatus(int id, int pagina = 1)
    {
        var resultado = await _model.CambiarCategoriaEstatusAsync(id);
        if (resultado.Estado == AdminEstado.NoEncontrado) return NotFound();
        return RedirectToAction(nameof(Categorias), new { pagina });
    }

    [HttpGet]
    public async Task<IActionResult> Cursos()
    {
        return View(await _model.CursosAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCurso(CursoCreateViewModel model)
    {
        var resultado = await _model.CrearCursoAsync(model, ModelState.IsValid);
        if (resultado.Estado == AdminEstado.FormularioInvalido)
        {
            return FormularioInvalido(resultado, "Cursos");
        }
        TempData["Mensaje"] = "Curso creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarCursoEstatus(int id)
    {
        var resultado = await _model.CambiarCursoEstatusAsync(id);
        if (resultado.Estado == AdminEstado.NoEncontrado) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Buscar(BuscarCursosViewModel model, int pagina = 1)
    {
        return View(await _model.BuscarAsync(model, pagina));
    }

    [HttpGet]
    public IActionResult Usuarios() => RedirectToAction(nameof(Empleados));

    [HttpGet]
    public async Task<IActionResult> Empleados(string? q, string? estatus, int pagina = 1, int? numero = null)
    {
        if (estatus is not (null or "ACTIVO" or "INACTIVO") || numero is <= 0)
            return BadRequest("Revisa el número de empleado y el estatus.");
        return View(await _model.EmpleadosAsync(q, estatus, pagina, numero));
    }

    [HttpGet]
    public async Task<IActionResult> RegistrarEmpleado()
    {
        return View("FormularioEmpleado", await _model.RegistrarEmpleadoAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarEmpleado(EmpleadoFormViewModel model)
    {
        model.Id = 0;
        ModelState.Remove(nameof(model.Id));
        var resultado = await _model.RegistrarEmpleadoAsync(model, ModelState.IsValid);
        if (resultado.Estado == AdminEstado.FormularioInvalido)
        {
            return FormularioInvalido(resultado, "FormularioEmpleado");
        }
        TempData["Mensaje"] = $"Empleado registrado correctamente (No. {resultado.Id}).";
        return RedirectToAction(nameof(Empleados), new { numero = resultado.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditarEmpleado(int id)
    {
        var model = await _model.EditarEmpleadoAsync(id);
        return model is null ? NotFound() : View("FormularioEmpleado", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarEmpleado(EmpleadoFormViewModel model)
    {
        var resultado = await _model.EditarEmpleadoAsync(model, ModelState.IsValid);
        if (resultado.Estado == AdminEstado.NoEncontrado) return NotFound();
        if (resultado.Estado == AdminEstado.FormularioInvalido)
        {
            return FormularioInvalido(resultado, "FormularioEmpleado");
        }
        TempData["Mensaje"] = "Datos del empleado actualizados correctamente.";
        return RedirectToAction(nameof(Empleados), new { numero = resultado.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarEmpleado(int id)
    {
        var resultado = await _model.EliminarEmpleadoAsync(id);
        if (resultado.Estado == AdminEstado.Conflicto) return Conflict(resultado.Mensaje);
        TempData["Mensaje"] = "El empleado se eliminó definitivamente.";
        return RedirectToAction(nameof(Empleados));
    }

    [HttpGet]
    public async Task<IActionResult> Catalogos(string tipo = "sexos", string? editar = null, int pagina = 1)
    {
        tipo = tipo.ToLowerInvariant();
        if (!AdminModel.EsTipoCatalogoValido(tipo)) return NotFound();
        return View(await _model.CatalogosAsync(tipo, editar, pagina));
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
        var resultado = await _model.GuardarCatalogoAsync(elemento, tipo, claveOriginal, ModelState.IsValid, pagina);
        if (resultado.Estado == AdminEstado.NoEncontrado) return NotFound();
        if (resultado.Estado == AdminEstado.DatosInvalidos) return BadRequest();
        if (resultado.Estado == AdminEstado.FormularioInvalido)
        {
            return FormularioInvalido(resultado, "Catalogos");
        }
        TempData["Mensaje"] = !string.IsNullOrWhiteSpace(claveOriginal) ? "Elemento actualizado correctamente." : "Elemento agregado correctamente.";
        return RedirectToAction(nameof(Catalogos), new { tipo, pagina });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarCatalogo(string tipo, string clave, int pagina = 1)
    {
        tipo = tipo.ToLowerInvariant();
        var resultado = await _model.EliminarCatalogoAsync(tipo, clave);
        if (resultado.Estado == AdminEstado.NoEncontrado) return NotFound();
        if (resultado.Estado == AdminEstado.DatosInvalidos) return BadRequest();
        if (resultado.Estado == AdminEstado.Conflicto) return Conflict(resultado.Mensaje);
        TempData["Mensaje"] = "Estatus del elemento actualizado; se conserva el historial asociado.";
        return RedirectToAction(nameof(Catalogos), new { tipo, pagina });
    }

    [HttpGet]
    public async Task<IActionResult> DetalleUsuario(int id)
    {
        var model = await _model.DetalleUsuarioAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public IActionResult Registro() => RedirectToAction(nameof(Empleados));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistroUsuarioViewModel model)
    {
        var resultado = await _model.RegistrarAsync(model, ModelState.IsValid);
        if (resultado.Estado == AdminEstado.FormularioInvalido)
        {
            return FormularioInvalido(resultado, "Registro");
        }
        TempData["Mensaje"] = $"Usuario registrado correctamente (ID {resultado.Id}).";
        return RedirectToAction(nameof(Empleados));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarUsuarioEstatus(int id)
    {
        var resultado = await _model.CambiarUsuarioEstatusAsync(id);
        if (resultado.Estado == AdminEstado.NoEncontrado) return NotFound();
        TempData["Mensaje"] = "Empleado dado de baja.";
        return RedirectToAction(nameof(Empleados), new { numero = id });
    }

    [HttpGet]
    public async Task<IActionResult> AsignarCurso(int id)
    {
        var model = await _model.AsignarCursoAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarCurso(AsignarCursoViewModel model)
    {
        var resultado = await _model.AsignarCursoAsync(model, ModelState.IsValid);
        if (resultado.Estado == AdminEstado.NoEncontrado) return NotFound();
        if (resultado.Estado == AdminEstado.Conflicto) return Conflict(resultado.Mensaje);
        if (resultado.Estado == AdminEstado.FormularioInvalido)
        {
            return FormularioInvalido(resultado);
        }
        TempData["Mensaje"] = "Curso asignado correctamente.";
        return RedirectToAction(nameof(DetalleUsuario), new { id = model.UsuarioId });
    }
    private IActionResult FormularioInvalido(AdminResultado resultado, string? vista = null)
    {
        foreach (var error in resultado.Errores)
            ModelState.AddModelError(error.Campo, error.Mensaje);
        return View(vista, resultado.Formulario);
    }
}
