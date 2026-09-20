namespace NoSqlU.Data.Entities;

public class Instructor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Apellido { get; set; }
    public string? Email { get; set; }
    public string Estatus { get; set; } = "activo";
    public DateTime FechaCreacion { get; set; }

    public ICollection<Curso>? Cursos { get; set; }
}
