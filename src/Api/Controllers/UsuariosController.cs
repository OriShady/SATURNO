using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;
using NoSqlU.Data;
using NoSqlU.Data.Entities;
using NoSqlU.Api.Models.Validation;

namespace NoSqlU.Api.Controllers;

[ApiController]

[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly SaturnoStore _db;

    public UsuariosController(SaturnoStore db) => _db = db;

    [HttpGet]
    public async Task<ActionResult> GetUsuarios(int? numero, string? nombre, string? estatus, int pagina = 1)
    {
        if (numero is <= 0 || estatus is not (null or "ACTIVO" or "INACTIVO"))
            return BadRequest(new { error = "Revisa el número de empleado y el estatus." });
        if (numero is null && string.IsNullOrWhiteSpace(nombre) && estatus is null)
            return Ok(Array.Empty<object>());

        var query = _db.FiltrarEmpleados(nombre, numero, estatus);
        var usuarios = await query.OrderBy(u => u.Id).Skip((Math.Clamp(pagina, 1, 1_000_000) - 1) * 20).Take(20)
            .Select(u => new
            {
                u.Id,
                NombreCompleto = u.Nombres + " " + (u.ApellidoPaterno ?? "") + " " + (u.ApellidoMaterno ?? ""),
                u.CorreoEmpresarial,
                u.Telefono,
                u.Estatus
            })
            .ToListAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetUsuario(int id)
    {
        var u = await _db.BuscarDetalleEmpleadoAsync(id);
        if (u is null) return NotFound(new { error = "Usuario no encontrado." });
        var usuario = new
            {
                u.Id,
                u.Nombres,
                u.ApellidoPaterno,
                u.ApellidoMaterno,
                u.FechaNacimiento,
                Sexo = u.Sexo?.Descripcion,
                u.CorreoEmpresarial,
                u.Telefono,
                u.FechaRegistro,
                u.Estatus,
                UsuarioLogin = u.Contrasena == null ? null : u.Contrasena.UsuarioLogin,
                Direcciones = u.Direcciones.Select(d => new
                {
                    d.Calle,
                    d.NumeroExterior,
                    d.NumeroInterior,
                    d.CodigoPostal,
                    d.Colonia,
                    d.Municipio,
                    Estado = d.Estado?.NombreEstado
                }),
                Cursos = u.CursosUsuarios.Select(c => new
                {
                    Curso = c.NombreCurso,
                    c.DuracionMinutos,
                    c.FechaEntregaConstancia,
                    c.Institucion,
                    c.FechaInicio,
                    c.FechaFinalizacion,
                    c.Estatus
                })
            };
        return Ok(usuario);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Eliminar(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);

        if (usuario == null) return NotFound(new { error = "Usuario no encontrado." });

        if (!await _db.EliminarEmpleadoInactivoAsync(id))
            return Conflict(new { error = "Solo se puede eliminar definitivamente a un empleado inactivo." });

        return NoContent();
    }

    [HttpGet("estados")]
    public async Task<ActionResult> GetEstados()
    {
        var estados = await _db.Estados.Query.Where(e => e.Estatus == "ACTIVO").OrderBy(e => e.NombreEstado).ToListAsync();
        return Ok(estados);
    }

    [HttpPost("registro")]
    public async Task<ActionResult> Registrar([FromBody] RegistroUsuarioDto dto)
    {
        // 1. Validaciones para evitar duplicados
        if (await _db.Usuarios.Query.AnyAsync(u => u.CorreoEmpresarial == dto.CorreoEmpresarial))
            return BadRequest(new { error = "El correo empresarial ya está registrado." });

        if (await _db.LoginExistsAsync(dto.UsuarioLogin))
            return BadRequest(new { error = "El nombre de usuario (login) ya existe." });

        if (string.IsNullOrWhiteSpace(dto.ApellidoPaterno) && string.IsNullOrWhiteSpace(dto.ApellidoMaterno))
            return BadRequest(new { error = "Debe proporcionar al menos un apellido (paterno o materno)." });

        if (await _db.Sexos.FindAsync(dto.SexoId) is null || await _db.Estados.FindAsync(dto.EstadoId) is null)
            return BadRequest(new { error = "Sexo o estado inexistente." });

        // 2. Crear la entidad principal del Usuario
        var nuevoUsuario = new Usuario
        {
            Nombres = dto.Nombres,
            ApellidoPaterno = dto.ApellidoPaterno,
            ApellidoMaterno = dto.ApellidoMaterno,
            FechaNacimiento = dto.FechaNacimiento!.Value,
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
        await _db.Usuarios.InsertAsync(nuevoUsuario);

        return Ok(new { mensaje = "Usuario registrado exitosamente", idUsuario = nuevoUsuario.Id });
    }

    [HttpPut("{id}/estatus")]
    public async Task<ActionResult> CambiarEstatus(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound(new { error = "Usuario no encontrado." });

        usuario.Estatus = usuario.Estatus == "ACTIVO" ? "INACTIVO" : "ACTIVO";
        await _db.CambiarEstatusEmpleadoAsync(usuario);

        return Ok(new { mensaje = "Estatus actualizado correctamente", estatus = usuario.Estatus });
    }

    [HttpGet("cursos-disponibles")]
    public async Task<ActionResult> GetCursosDisponibles()
    {
        // Traer los cursos activos en el menu
        var cursos = await _db.Cursos.Query
            .Where(c => c.Estatus == "activo")
            .Select(c => new { c.Id, c.Nombre })
            .ToListAsync();

        return Ok(cursos);
    }

    [HttpPost("asignar-curso")]
    public async Task<ActionResult> AsignarCurso([FromBody] AsignarCursoDto dto)
    {
        // 1. Validar que no esté ya inscrito en ese mismo curso
        if (await _db.Usuarios.FindAsync(dto.UsuarioId) is null) return NotFound();
        bool yaInscrito = await _db.TieneCursoAsync(dto.UsuarioId, dto.CursoId);

        if (yaInscrito) 
            return BadRequest(new { error = "El usuario ya está inscrito en este curso." });

        // 2. Crear la relación
        var curso = await _db.Cursos.FindAsync(dto.CursoId);
        if (curso == null || curso.Estatus != "activo")
            return BadRequest(new { error = "El curso seleccionado no existe o está inactivo." });

        var nuevaInscripcion = new CursoUsuario
        {
            CursoId = dto.CursoId,
            NombreCurso = curso.Nombre,
            DuracionMinutos = curso.DuracionMinutos,
            FechaInicio = DateTime.UtcNow,
            Estatus = "EN CURSO" 
        };

        if (!await _db.AsignarCursoAsync(dto.UsuarioId, nuevaInscripcion))
            return Conflict(new { error = "La inscripción ya existe o el empleado fue eliminado." });

        return Ok(new { mensaje = "Curso asignado correctamente al usuario." });
    }
}



// Objeto de transferencia de datos (DTO) para recibir la información limpia del cliente
public class RegistroUsuarioDto
{
    public string Nombres { get; set; } = null!;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [MinimumAge(18, ErrorMessage = "El empleado debe tener al menos 18 años.")]
    public DateTime? FechaNacimiento { get; set; }
    public char SexoId { get; set; }
    [Required(ErrorMessage = "El correo empresarial es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido, por ejemplo nombre@dominio.com.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
    public string CorreoEmpresarial { get; set; } = null!;
    [RegularExpression(@"^(?:[0-9]{10})?$", ErrorMessage = "Ingresa 10 dígitos, sin espacios ni guiones.")]
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
