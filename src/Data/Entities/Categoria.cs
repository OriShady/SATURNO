namespace NoSqlU.Data.Entities;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string Estatus { get; set; } = "activo"; // 'activo'|'inactivo'
    public DateTime FechaCreacion { get; set; }

    public ICollection<Curso>? Cursos { get; set; }
}
