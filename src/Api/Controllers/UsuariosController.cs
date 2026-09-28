using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoSqlU.Data;
using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Controllers;

[ApiController]

[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly NoSqlUContext _db;
    
    public UsuariosController(NoSqlUContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult> GetUsuarios()
    {
        var usuarios = await _db.Usuarios
            .Select(u => new
            {
                u.Id,
                NombreCompleto = u.Nombres + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno,
                u.CorreoEmpresarial,
                u.Telefono,
                u.Estatus
            })
            .ToListAsync();
        return Ok(usuarios);
    }

    [HttpGet("estados")]
    public async Task<ActionResult> GetEstados()
    {
        var estados = await _db.Estados.OrderBy(e => e.NombreEstado).ToListAsync();
        return Ok(estados);
    }

    [HttpPost("registro")]
    public async Task<ActionResult> Registrar([FromBody] RegistroUsuarioDto dto)
    {
        // 1. Validaciones para evitar duplicados
        if (await _db.Usuarios.AnyAsync(u => u.CorreoEmpresarial == dto.CorreoEmpresarial))
            return BadRequest(new { error = "El correo empresarial ya está registrado." });

        if (await _db.UsuariosContrasenas.AnyAsync(uc => uc.UsuarioLogin == dto.UsuarioLogin))
            return BadRequest(new { error = "El nombre de usuario (login) ya existe." });

        if (string.IsNullOrWhiteSpace(dto.ApellidoPaterno) && string.IsNullOrWhiteSpace(dto.ApellidoMaterno))
            return BadRequest(new { error = "Debe proporcionar al menos un apellido (paterno o materno)." });

        // 2. Crear la entidad principal del Usuario
        var nuevoUsuario = new Usuario
        {
            Nombres = dto.Nombres,
            ApellidoPaterno = dto.ApellidoPaterno,
            ApellidoMaterno = dto.ApellidoMaterno,
            FechaNacimiento = dto.FechaNacimiento,
            SexoId = dto.SexoId,
            CorreoEmpresarial = dto.CorreoEmpresarial,
            Telefono = dto.Telefono,
            Estatus = "ACTIVO"
        };

        // 3. Generar Hash de la contraseña
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(dto.Password);
        var hash = Convert.ToBase64String(sha256.ComputeHash(bytes));

        // 4. Crear y asignar la entidad relacionada de la contraseña
        nuevoUsuario.Contrasena = new UsuarioContrasena
        {
            UsuarioLogin = dto.UsuarioLogin,
            PasswordHash = hash
        };

        // 4.5 Crear y asignar la entidad de dirección
        nuevoUsuario.Direcciones.Add(new DireccionUsuario
        {
            EstadoId = dto.EstadoId,
            Calle = dto.Calle,
            NumeroExterior = dto.NumeroExterior,
            NumeroInterior = dto.NumeroInterior,
            CodigoPostal = dto.CodigoPostal,
            Colonia = dto.Colonia,
            Municipio = dto.Municipio
        });

        // 5. Guardar todo en la base de datos dentro de una transacción implícita
        _db.Usuarios.Add(nuevoUsuario);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Usuario registrado exitosamente", idUsuario = nuevoUsuario.Id });
    }

    [HttpPut("{id}/estatus")]
    public async Task<ActionResult> CambiarEstatus(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound(new { error = "Usuario no encontrado." });

        usuario.Estatus = usuario.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Estatus actualizado correctamente", estatus = usuario.Estatus });
    }

    [HttpGet("cursos-disponibles")]
    public async Task<ActionResult> GetCursosDisponibles()
    {
        // Traer los cursos activos en el menu
        var cursos = await _db.Cursos
            .Where(c => c.Estatus == "activo")
            .Select(c => new { c.Id, c.Nombre })
            .ToListAsync();
        
        return Ok(cursos);
    }

    [HttpPost("asignar-curso")]
    public async Task<ActionResult> AsignarCurso([FromBody] AsignarCursoDto dto)
    {
        // 1. Validar que no esté ya inscrito en ese mismo curso
        bool yaInscrito = await _db.CursosUsuarios
            .AnyAsync(cu => cu.UsuarioId == dto.UsuarioId && cu.CursoId == dto.CursoId);
            
        if (yaInscrito) 
            return BadRequest(new { error = "El usuario ya está inscrito en este curso." });

        // 2. Crear la relación
        var nuevaInscripcion = new CursoUsuario
        {
            UsuarioId = dto.UsuarioId,
            CursoId = dto.CursoId,
            FechaInicio = DateTime.UtcNow,
            Estatus = "EN CURSO" 
        };

        _db.CursosUsuarios.Add(nuevaInscripcion);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Curso asignado correctamente al usuario." });
    }
}



// Objeto de transferencia de datos (DTO) para recibir la información limpia del cliente
public class RegistroUsuarioDto
{
    public string Nombres { get; set; } = null!;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public char SexoId { get; set; }
    public string CorreoEmpresarial { get; set; } = null!;
    public string? Telefono { get; set; }
    public string UsuarioLogin { get; set; } = null!;
    public string Password { get; set; } = null!;

    public int EstadoId { get; set; }
    public string Calle { get; set; } = null!;
    public string NumeroExterior { get; set; } = null!;
    public string? NumeroInterior { get; set; }
    public string CodigoPostal { get; set; } = null!;
    public string Colonia { get; set; } = null!;
    public string Municipio { get; set; } = null!;
}


public class AsignarCursoDto
{
    public int UsuarioId { get; set; }
    public int CursoId { get; set; }
}

