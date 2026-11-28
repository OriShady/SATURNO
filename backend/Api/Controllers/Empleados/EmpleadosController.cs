using Microsoft.AspNetCore.Mvc;
using NoSqlU.Api.Models.Empleados;

namespace NoSqlU.Api.Controllers.Empleados;

public sealed class EmpleadosController : Controller
{
    private readonly EmpleadosService _empleados;

    public EmpleadosController(EmpleadosService empleados) => _empleados = empleados;

    [HttpGet]
    public async Task<IActionResult> Empleados(string? q, string? estatus, int pagina = 1, int? numero = null)
    {
        if (estatus is not (null or "ACTIVO" or "INACTIVO") || numero is <= 0)
            return BadRequest("Revisa el número de empleado y el estatus.");
        return View(await _empleados.ListarAsync(q, estatus, pagina, numero));
    }

    [HttpGet]
    public async Task<IActionResult> RegistrarEmpleado() => View("FormularioEmpleado", await _empleados.PrepararFormularioAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarEmpleado(EmpleadoFormViewModel model)
    {
        model.Id = 0;
        ModelState.Remove(nameof(model.Id));
        foreach (var (campo, mensaje) in await _empleados.ValidarAsync(model))
            ModelState.AddModelError(campo, mensaje);
        if (!ModelState.IsValid)
            return View("FormularioEmpleado", await _empleados.PrepararFormularioAsync(model));

        var id = await _empleados.RegistrarAsync(model);
        TempData["Mensaje"] = $"Empleado registrado correctamente (No. {id}).";
        return RedirectToAction(nameof(Empleados), new { numero = id });
    }

    [HttpGet]
    public async Task<IActionResult> EditarEmpleado(int id)
    {
        var model = await _empleados.PrepararEdicionAsync(id);
        return model is null ? NotFound() : View("FormularioEmpleado", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarEmpleado(EmpleadoFormViewModel model)
    {
        if (!await _empleados.ExisteAsync(model.Id)) return NotFound();

        foreach (var (campo, mensaje) in await _empleados.ValidarAsync(model, model.Id))
            ModelState.AddModelError(campo, mensaje);
        if (!ModelState.IsValid)
            return View("FormularioEmpleado", await _empleados.PrepararFormularioAsync(model));

        if (!await _empleados.EditarAsync(model)) return NotFound();
        TempData["Mensaje"] = "Datos del empleado actualizados correctamente.";
        return RedirectToAction(nameof(Empleados), new { numero = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarEmpleado(int id)
    {
        if (!await _empleados.EliminarAsync(id))
            return Conflict("Solo se puede eliminar definitivamente a un empleado inactivo.");
        TempData["Mensaje"] = "El empleado se eliminó definitivamente.";
        return RedirectToAction(nameof(Empleados));
    }

    [HttpGet]
    public async Task<IActionResult> DetalleUsuario(int id)
    {
        var usuario = await _empleados.BuscarDetalleAsync(id);

        return usuario is null ? NotFound() : View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarUsuarioEstatus(int id)
    {
        if (!await _empleados.DarDeBajaAsync(id)) return NotFound();
        TempData["Mensaje"] = "Empleado dado de baja.";
        return RedirectToAction(nameof(Empleados), new { numero = id });
    }
}
