using Microsoft.EntityFrameworkCore;
using NoSqlU.Data;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Enable CORS for local frontend (development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=NoSqlU1_migrated;Trusted_Connection=True;";
builder.Services.AddDbContext<NoSqlUContext>(options => options.UseSqlServer(connection));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply CORS before HTTPS redirection so redirect responses include CORS headers
app.UseCors("AllowAll");
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
// Serve frontend static files from /admin (serves frontend/admin folder)
var frontendAdminPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "frontend", "admin"));
if (Directory.Exists(frontendAdminPath))
{
    app.UseFileServer(new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(frontendAdminPath),
        RequestPath = "/admin",
        EnableDefaultFiles = true
    });
}
app.MapControllers();

app.Run();
