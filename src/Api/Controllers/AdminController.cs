using Microsoft.AspNetCore.Mvc;

namespace NoSqlU.Api.Controllers;

public class AdminController : Controller
{
    [HttpGet("/")]
    [HttpGet("/admin")]
    [HttpGet("/admin/index.html")]
    public IActionResult Index()
    {
        ViewData["ActivePage"] = "dashboard";
        return View();
    }

    [HttpGet("/admin/categorias")]
    [HttpGet("/admin/categorias.html")]
    public IActionResult Categorias()
    {
        ViewData["ActivePage"] = "categorias";
        return View();
    }

    [HttpGet("/admin/cursos")]
    [HttpGet("/admin/cursos.html")]
    public IActionResult Cursos()
    {
        ViewData["ActivePage"] = "cursos";
        return View();
    }

    [HttpGet("/admin/busqueda")]
    [HttpGet("/admin/busqueda.html")]
    public IActionResult Busqueda()
    {
        ViewData["ActivePage"] = "busqueda";
        return View();
    }
}