using System;
using System.Collections.Generic;

namespace NoSqlU.Data.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombres { get; set; } = null!;
    public string? ApellidoPaterno { get; set; }
    public string? ApellidoMaterno { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public char SexoId { get; set; }
    public string CorreoEmpresarial { get; set; } = null!;
    public string? Telefono { get; set; }
    public DateTime FechaInscripcion { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string Estatus { get; set; } = "ACTIVO";

    public Sexo Sexo { get; set; } = null!;
    public UsuarioContrasena? Contrasena { get; set; }
    public ICollection <CursoUsuario> CursosUsuarios { get; set; } = new List <CursoUsuario>();
    public ICollection <DireccionUsuario> Direcciones { get; set; } = new List <DireccionUsuario>();
}