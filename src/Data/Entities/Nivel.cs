namespace NoSqlU.Data.Entities;

public class Nivel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Estatus { get; set; } = "activo";
    public DateTime FechaCreacion { get; set; }

    public ICollection<Curso>? Cursos { get; set; }
}
