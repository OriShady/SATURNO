using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSqlU.Data.Entities;

public class UsuarioContrasena
{

    [BsonElement("usuario")]
    public string UsuarioLogin { get; set; } = null!;
    [BsonElement("password_hash")]
    public string PasswordHash { get; set; } = null!;
    [BsonIgnore]
    public Usuario Usuario { get; set; } = null!;
}