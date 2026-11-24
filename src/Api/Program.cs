using MongoDB.Driver;
using NoSqlU.Data;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
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
app.UseAuthorization();
// Keep the existing stylesheet available while the interface is rendered by MVC.
var frontendCssPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "frontend", "css"));
if (Directory.Exists(frontendCssPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendCssPath),
        RequestPath = "/css"
    });
}
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Empleados}/{id?}");

app.Run();
