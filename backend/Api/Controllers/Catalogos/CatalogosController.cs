using Microsoft.AspNetCore.Mvc;
using NoSqlU.Api.Models.Catalogos;

namespace NoSqlU.Api.Controllers.Catalogos;

public sealed class CatalogosController : Controller
{
    private readonly CatalogosService _catalogos;

    public CatalogosController(CatalogosService catalogos) => _catalogos = catalogos;

    [HttpGet]
    public async Task<IActionResult> Catalogos(string tipo = "sexos", string? editar = null, int pagina = 1)
    {
        tipo = tipo.ToLowerInvariant();
        if (!EsTipoCatalogoValido(tipo)) return NotFound();
        return View(await _catalogos.PrepararVistaAsync(tipo, editar, pagina: pagina, mostrarFormulario: editar is not null));
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
            var duplicado = await _catalogos.ExisteDuplicadoAsync(tipo, elemento.Nombre.Trim(), elemento.Clave?.Trim(), claveOriginal);
            if (duplicado)
                ModelState.AddModelError("Elemento.Nombre", "Ya existe un elemento con ese nombre o clave.");
        }

        if (!ModelState.IsValid)
            return View("Catalogos", await _catalogos.PrepararVistaAsync(tipo, claveOriginal, elemento, mostrarFormulario: true, pagina: pagina));

        var (encontrado, errorClave) = await _catalogos.GuardarAsync(tipo, elemento, claveOriginal);
        if (!encontrado) return NotFound();
        if (errorClave is not null)
        {
            ModelState.AddModelError("Elemento.Clave", errorClave);
            return View("Catalogos", await _catalogos.PrepararVistaAsync(tipo, claveOriginal, elemento, mostrarFormulario: true, pagina: pagina));
        }

        var editar = !string.IsNullOrWhiteSpace(claveOriginal);
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
        var (encontrado, conflicto) = await _catalogos.CambiarEstatusAsync(tipo, clave);
        if (!encontrado) return NotFound();
        if (conflicto is not null) return Conflict(conflicto);

        TempData["Mensaje"] = "Estatus del elemento actualizado; se conserva el historial asociado.";
        return RedirectToAction(nameof(Catalogos), new { tipo, pagina });
    }

    private static bool EsTipoCatalogoValido(string tipo) => tipo is "sexos" or "estados";
}
