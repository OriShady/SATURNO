using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSqlU.Data.Entities;

public class Sexo
{
    [BsonRepresentation(BsonType.String)]
    public char Id { get; set; }
    [BsonElement("nombre")]
    public string Descripcion { get; set; } = null!;
    [BsonElement("estatus")]
    public string Estatus { get; set; } = "ACTIVO";
}