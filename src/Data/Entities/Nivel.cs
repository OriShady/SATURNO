using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSqlU.Data.Entities;

public class Nivel
{
    public int Id { get; set; }
    [BsonElement("nombre")]
    public string Nombre { get; set; } = null!;
    [BsonElement("estatus")]
    public string Estatus { get; set; } = "activo";
    [BsonElement("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [BsonIgnore]
    public ICollection<Curso>? Cursos { get; set; }
}
