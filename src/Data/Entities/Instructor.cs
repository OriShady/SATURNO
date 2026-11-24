using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSqlU.Data.Entities;

public class Instructor
{
    public int Id { get; set; }
    [BsonElement("nombre")]
    public string Nombre { get; set; } = null!;
    [BsonElement("apellido")]
    public string? Apellido { get; set; }
    [BsonElement("email")]
    public string? Email { get; set; }
    [BsonElement("estatus")]
    public string Estatus { get; set; } = "activo";
    [BsonElement("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [BsonIgnore]
    public ICollection<Curso>? Cursos { get; set; }
}
