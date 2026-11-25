using MongoDB.Bson.Serialization.Attributes;

namespace NoSqlU.Data.Entities;

public sealed class Administrador
{
    [BsonId]
    public string Usuario { get; set; } = string.Empty;

    [BsonElement("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("estatus")]
    public string Estatus { get; set; } = "ACTIVO";
}
