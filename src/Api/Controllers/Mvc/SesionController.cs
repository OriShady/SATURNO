using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Controllers.Mvc;

public sealed class SesionController : Controller
{
    private readonly SaturnoStore store;
    private readonly IPasswordHasher<Administrador> hasher;

    public SesionController(SaturnoStore store, IPasswordHasher<Administrador> hasher)
    {
        this.store = store;
        this.hasher = hasher;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Entrar() => User.Identity?.IsAuthenticated == true
        ? RedirectToAction("Empleados", "Admin")
        : View();

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrar(string usuario, string password)
    {
        var administrador = string.IsNullOrWhiteSpace(usuario) ? null :
            await store.BuscarAdministradorAsync(usuario.Trim().ToLowerInvariant());
        if (administrador is null || string.IsNullOrEmpty(password) ||
            hasher.VerifyHashedPassword(administrador, administrador.PasswordHash, password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View();
        }

        var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, administrador.Usuario) }, scheme);
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
