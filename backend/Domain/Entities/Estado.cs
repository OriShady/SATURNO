using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSqlU.Domain.Entities;

public class Estado
{
    public int Id { get; set; }
    [BsonElement("nombre")]
    public string NombreEstado { get; set; } = null!;
    [BsonElement("estatus")]
    public string Estatus { get; set; } = "ACTIVO";
}