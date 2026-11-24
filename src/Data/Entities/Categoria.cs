using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSqlU.Data.Entities;

public class Categoria
{
    public int Id { get; set; }
    [BsonElement("nombre")]
    public string Nombre { get; set; } = null!;
    [BsonElement("descripcion")]
    public string? Descripcion { get; set; }
    [BsonElement("estatus")]
    public string Estatus { get; set; } = "activo"; // 'activo'|'inactivo'
    [BsonElement("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [BsonIgnore]
    public ICollection<Curso>? Cursos { get; set; }
}
