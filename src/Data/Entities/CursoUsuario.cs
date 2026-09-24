using System;

namespace NoSqlU.Data.Entities;

public class CursoUsuario
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int CursoId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public string Estatus { get; set; } = "EN CURSO";

    public Usuario Usuario { get; set; } = null!;
    public Curso Curso { get; set; } = null!;
}