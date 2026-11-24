using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSqlU.Data.Entities;

public class Curso
{
    public int Id { get; set; }
    [BsonElement("id_categoria")]
    public int CategoriaId { get; set; }
    [BsonElement("id_nivel")]
    public int NivelId { get; set; }
    [BsonElement("id_instructor")]
    public int InstructorId { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; } = null!;
    [BsonElement("descripcion")]
    public string Descripcion { get; set; } = null!;
    [BsonElement("precio")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Precio { get; set; }
    [BsonElement("duracion_minutos")]
    public int DuracionMinutos { get; set; }
    [BsonElement("fecha_publicacion")]
    public DateTime FechaPublicacion { get; set; }
    [BsonElement("estatus")]
    public string Estatus { get; set; } = "activo";
    [BsonElement("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [BsonIgnore]
    public Categoria? Categoria { get; set; }
    [BsonIgnore]
    public Nivel? Nivel { get; set; }
    [BsonIgnore]
    public Instructor? Instructor { get; set; }
}

// Cambio para rama Patron 