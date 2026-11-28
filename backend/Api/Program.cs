using MongoDB.Driver;
using NoSqlU.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NoSqlU.Domain.Entities;
using NoSqlU.Api.Models.Catalogos;
using NoSqlU.Api.Models.Empleados;
using NoSqlU.Api.Models.Sesion;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
});

// Configuration
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IPasswordHasher<Administrador>, PasswordHasher<Administrador>>();
builder.Services.AddScoped<EmpleadosService>();
builder.Services.AddScoped<CatalogosService>();
builder.Services.AddScoped<SesionService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Sesion/Entrar";
});
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
// Enable CORS for local frontend (development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var mongoUri = builder.Configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
var mongoDatabase = builder.Configuration["MongoDB:Database"] ?? "SaturnoMongo";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoUri));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabase));
builder.Services.AddSingleton<SaturnoStore>();

var app = builder.Build();

var adminInicial = builder.Configuration["AdministradorInicial:Usuario"]?.Trim().ToLowerInvariant();
var claveInicial = builder.Configuration["AdministradorInicial:Password"];
if (!string.IsNullOrWhiteSpace(adminInicial) && !string.IsNullOrWhiteSpace(claveInicial))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<SesionService>().CrearAdministradorInicialAsync(adminInicial, claveInicial);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Apply CORS before HTTPS redirection so redirect responses include CORS headers
app.UseCors("AllowAll");
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (MongoWriteException error) when (!context.Response.HasStarted &&
        (error.WriteError.Category == ServerErrorCategory.DuplicateKey || error.WriteError.Code == 121))
    {
        var duplicado = error.WriteError.Category == ServerErrorCategory.DuplicateKey;
        context.Response.StatusCode = duplicado ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            error = duplicado ? "Ya existe un registro con esos datos." : "Los datos no cumplen la validación de la base."
        });
    }
});
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "catalogos",
    pattern: "Catalogos/{action=Catalogos}/{id?}",
    defaults: new { controller = "Catalogos" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Empleados}/{action=Empleados}/{id?}");
// Las rutas anteriores siguen aceptando enlaces y formularios del panel.
app.MapControllerRoute(
    name: "admin-catalogos",
    pattern: "Admin/{action}/{id?}",
    defaults: new { controller = "Catalogos" },
    constraints: new { action = "Catalogos|GuardarCatalogo|EliminarCatalogo" });
app.MapControllerRoute(
    name: "admin-empleados",
    pattern: "Admin/{action=Empleados}/{id?}",
    defaults: new { controller = "Empleados" });

app.Run();
