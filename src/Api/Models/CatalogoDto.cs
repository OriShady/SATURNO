using NoSqlU.Data.Entities;

namespace NoSqlU.Api.Models;

public sealed record CategoriaDto(int Id, string Nombre, string? Descripcion, string Estatus, DateTime FechaCreacion);
public sealed record NivelDto(int Id, string Nombre, string Estatus, DateTime FechaCreacion);
public sealed record InstructorDto(int Id, string Nombre, string? Apellido, string? Email, string Estatus, DateTime FechaCreacion);
public sealed record CursoDto(int Id, int CategoriaId, int NivelId, int InstructorId, string Nombre, string Descripcion,
    decimal Precio, int DuracionMinutos, DateTime FechaPublicacion, string Estatus, DateTime FechaCreacion,
    CategoriaDto? Categoria, NivelDto? Nivel, InstructorDto? Instructor);
public sealed record EstadoDto(int Id, string NombreEstado);
public sealed record SexoDto(char Id, string Descripcion);

public sealed class NivelInput
{
    public string Nombre { get; set; } = null!;
    public string Estatus { get; set; } = "activo";
}

public sealed class InstructorInput
{
    public string Nombre { get; set; } = null!;
    public string? Apellido { get; set; }
    public string? Email { get; set; }
    public string Estatus { get; set; } = "activo";
}

internal static class CatalogoDto
{
    public static CategoriaDto From(Categoria c) => new(c.Id, c.Nombre, c.Descripcion, c.Estatus, c.FechaCreacion);
    public static NivelDto From(Nivel n) => new(n.Id, n.Nombre, n.Estatus, n.FechaCreacion);
    public static InstructorDto From(Instructor i) => new(i.Id, i.Nombre, i.Apellido, i.Email, i.Estatus, i.FechaCreacion);
    public static EstadoDto From(Estado e) => new(e.Id, e.NombreEstado);
    public static SexoDto From(Sexo s) => new(s.Id, s.Descripcion);
    public static CursoDto From(Curso c) => new(c.Id, c.CategoriaId, c.NivelId, c.InstructorId, c.Nombre, c.Descripcion,
        c.Precio, c.DuracionMinutos, c.FechaPublicacion, c.Estatus, c.FechaCreacion,
        c.Categoria is null ? null : From(c.Categoria), c.Nivel is null ? null : From(c.Nivel),
        c.Instructor is null ? null : From(c.Instructor));
}
