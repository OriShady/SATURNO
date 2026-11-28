using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoSqlU.Api.Controllers.Empleados;
using NoSqlU.Api.Models.Sesion;

namespace NoSqlU.Api.Controllers.Sesion;

public sealed class SesionController : Controller
{
    private readonly SesionService _sesion;

    public SesionController(SesionService sesion) => _sesion = sesion;

    [HttpGet, AllowAnonymous]
    public IActionResult Entrar() => User.Identity?.IsAuthenticated == true
        ? RedirectToAction(nameof(EmpleadosController.Empleados), "Empleados")
        : View();

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrar(string usuario, string password)
    {
        var usuarioAutenticado = await _sesion.AutenticarAsync(usuario, password);
        if (usuarioAutenticado is null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View();
        }

        var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, usuarioAutenticado) }, scheme);
        await HttpContext.SignInAsync(scheme, new ClaimsPrincipal(identity));
        return RedirectToAction(nameof(EmpleadosController.Empleados), "Empleados");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Entrar));
    }
}
