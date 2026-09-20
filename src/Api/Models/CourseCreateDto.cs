namespace NoSqlU.Api.Models;

public class CourseCreateDto
{
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public uint DuracionMinutos { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public int CategoriaId { get; set; }

    // Opciones: puede enviar NivelId o Nivel (nombre). Si envía nombre, se creará si no existe.
    public int? NivelId { get; set; }
    public string? Nivel { get; set; }

    // Opciones: puede enviar InstructorId o Instructor (nombre). Si envía nombre, se creará si no existe.
    public int? InstructorId { get; set; }
    public string? Instructor { get; set; }
}
