using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoSqlU.Api.Models;

namespace NoSqlU.Api.Controllers.Mvc;

public sealed class SesionController : Controller
{
    private readonly SesionModel _model;

    public SesionController(SesionModel model) => _model = model;

    [HttpGet, AllowAnonymous]
    public IActionResult Entrar() => User.Identity?.IsAuthenticated == true
        ? RedirectToAction("Empleados", "Admin")
        : View();

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrar(string usuario, string password)
    {
        var administrador = await _model.AutenticarAsync(usuario, password);
        if (administrador is null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View();
        }

        var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, administrador) }, scheme);
        await HttpContext.SignInAsync(scheme, new ClaimsPrincipal(identity));
        return RedirectToAction("Empleados", "Admin");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Entrar));
    }
}
