namespace NoSqlU.Data.Entities;

public class Curso
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public int NivelId { get; set; }
    public int InstructorId { get; set; }

    public string Nombre { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public decimal Precio { get; set; }
    public uint DuracionMinutos { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public string Estatus { get; set; } = "activo";
    public DateTime FechaCreacion { get; set; }

    public Categoria? Categoria { get; set; }
    public Nivel? Nivel { get; set; }
    public Instructor? Instructor { get; set; }
}
