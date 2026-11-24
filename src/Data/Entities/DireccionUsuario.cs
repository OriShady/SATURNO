using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSqlU.Data.Entities;

public class DireccionUsuario
{

    [BsonElement("id_estado")]
    public int EstadoId { get; set; }
    [BsonElement("calle")]
    public string Calle { get; set; } = null!;
    [BsonElement("numero_exterior")]
    public string NumeroExterior { get; set; } = null!;
    [BsonElement("numero_interior")]
    public string? NumeroInterior { get; set; }
    [BsonElement("codigo_postal")]
    public string CodigoPostal { get; set; } = null!;
    [BsonElement("colonia")]
    public string Colonia { get; set; } = null!;
    [BsonElement("municipio")]
    public string Municipio { get; set; } = null!;

    [BsonIgnore]
    public Usuario Usuario { get; set; } = null!;
    [BsonIgnore]
    public Estado Estado { get; set; } = null!;
}